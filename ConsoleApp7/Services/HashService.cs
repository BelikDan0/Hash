using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using HashSystem.Models;

namespace HashSystem.Services
{
    /// <summary>
    /// Сервис для вычисления хешей (SHA256, SHA512, MD5, HMAC), генерации соли,
    /// хеширования паролей с солью и ведения журнала операций.
    /// </summary>
    public class HashService
    {
        private List<HashLog> _logs;

        public HashService()
        {
            _logs = new List<HashLog>();
        }

        /// <summary>Записывает ошибку в журнал (вспомогательный метод).</summary>
        private void LogError(string operation, string algorithm, string inputPreview, Exception ex)
        {
            LogOperation(Guid.NewGuid().ToString(), operation, algorithm, inputPreview, null, false, ex.Message);
        }

        // ------------------ Работа со строками (текстовые данные) ------------------

        /// <summary>Вычисляет SHA-256 для строки (UTF-8). Возвращает HEX в верхнем регистре, 64 символа.</summary>
        /// <param name="input">Входная строка.</param>
        /// <returns>Хеш в виде HEX-строки.</returns>
        /// <exception cref="ArgumentNullException">Если input == null.</exception>
        public string ComputeSha256(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                return ComputeSha256(bytes);
            }
            catch (Exception ex)
            {
                LogError("ComputeSha256", "SHA256", input.Length > 20 ? input.Substring(0, 20) : input, ex);
                throw;
            }
        }

        /// <summary>Вычисляет SHA-512 для строки. Возвращает HEX, 128 символов.</summary>
        /// <param name="input">Входная строка.</param>
        /// <returns>Хеш в виде HEX-строки.</returns>
        /// <exception cref="ArgumentNullException">Если input == null.</exception>
        public string ComputeSha512(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return ComputeSha512(bytes);
        }

        /// <summary>Вычисляет MD5 для строки. Возвращает HEX, 32 символа.</summary>
        /// <param name="input">Входная строка.</param>
        /// <returns>Хеш в виде HEX-строки.</returns>
        /// <exception cref="ArgumentNullException">Если input == null.</exception>
        public string ComputeMd5(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return ComputeMd5(bytes);
        }

        // ------------------ Работа с байтами (бинарные данные) ------------------

        /// <summary>Вычисляет SHA-256 для массива байт.</summary>
        /// <param name="data">Бинарные данные.</param>
        /// <returns>HEX-строка (64 символа, верхний регистр).</returns>
        /// <exception cref="ArgumentNullException">Если data == null.</exception>
        public string ComputeSha256(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }

        /// <summary>Вычисляет SHA-512 для массива байт.</summary>
        /// <param name="data">Бинарные данные.</param>
        /// <returns>HEX-строка (128 символов, верхний регистр).</returns>
        /// <exception cref="ArgumentNullException">Если data == null.</exception>
        public string ComputeSha512(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            using var sha = SHA512.Create();
            byte[] hash = sha.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }

        /// <summary>Вычисляет MD5 для массива байт.</summary>
        /// <param name="data">Бинарные данные.</param>
        /// <returns>HEX-строка (32 символа, верхний регистр).</returns>
        /// <exception cref="ArgumentNullException">Если data == null.</exception>
        public string ComputeMd5(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            using var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }

        /// <summary>Вычисляет HMAC-SHA256 для сообщения с секретным ключом.</summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="secretKey">Секретный ключ.</param>
        /// <returns>HEX-строка (64 символа, верхний регистр).</returns>
        /// <exception cref="ArgumentNullException">Если message == null или secretKey пуст.</exception>
        public string ComputeHmacSha256(string message, string secretKey)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrEmpty(secretKey)) throw new ArgumentNullException(nameof(secretKey), "Secret key cannot be null or empty");

            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            using var hmac = new HMACSHA256(keyBytes);
            byte[] hash = hmac.ComputeHash(messageBytes);
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }

        /// <summary>Проверяет, соответствует ли входная строка ожидаемому хешу.</summary>
        /// <param name="input">Входная строка.</param>
        /// <param name="expectedHash">Ожидаемый хеш (HEX).</param>
        /// <param name="algorithm">Алгоритм (SHA256, SHA512, MD5).</param>
        /// <returns>true, если хеш совпадает; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Если любой из аргументов null.</exception>
        /// <exception cref="ArgumentException">Если algorithm пуст.</exception>
        /// <exception cref="InvalidDataException">Если длина expectedHash не соответствует алгоритму.</exception>
        /// <exception cref="NotSupportedException">Если алгоритм не поддерживается.</exception>
        public bool VerifyHash(string input, string expectedHash, string algorithm)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (expectedHash == null) throw new ArgumentNullException(nameof(expectedHash));
            if (string.IsNullOrEmpty(algorithm)) throw new ArgumentException("Algorithm cannot be empty", nameof(algorithm));

            algorithm = algorithm.ToUpperInvariant();
            if (algorithm == "SHA256" && expectedHash.Length != 64)
                throw new InvalidDataException($"SHA256 hash must be 64 characters, got {expectedHash.Length}");
            if (algorithm == "SHA512" && expectedHash.Length != 128)
                throw new InvalidDataException($"SHA512 hash must be 128 characters, got {expectedHash.Length}");
            if (algorithm == "MD5" && expectedHash.Length != 32)
                throw new InvalidDataException($"MD5 hash must be 32 characters, got {expectedHash.Length}");

            string actualHash = algorithm switch
            {
                "SHA256" => ComputeSha256(input),
                "SHA512" => ComputeSha512(input),
                "MD5" => ComputeMd5(input),
                _ => throw new NotSupportedException($"Algorithm '{algorithm}' is not supported.")
            };
            return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Генерирует случайную соль заданной длины (в байтах) и возвращает её в Base64.</summary>
        /// <param name="length">Длина соли в байтах (по умолчанию 16).</param>
        /// <returns>Соль в формате Base64.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Если length ≤ 0 или больше 1024.</exception>
        public string GenerateSalt(int length = 16)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length), "Salt length must be positive");
            if (length > 1024) throw new ArgumentOutOfRangeException(nameof(length), "Salt length too large (max 1024)");
            byte[] bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>Хеширует строку с добавлением соли (порядок: input + salt).</summary>
        /// <param name="input">Исходная строка (пароль).</param>
        /// <param name="salt">Соль.</param>
        /// <param name="algorithm">Алгоритм (SHA256, SHA512, MD5).</param>
        /// <returns>Хеш в виде HEX-строки.</returns>
        /// <exception cref="ArgumentNullException">Если input == null или salt пуст.</exception>
        /// <exception cref="ArgumentException">Если algorithm пуст.</exception>
        /// <exception cref="NotSupportedException">Если алгоритм не поддерживается.</exception>
        public string HashWithSalt(string input, string salt, string algorithm)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (string.IsNullOrEmpty(salt)) throw new ArgumentNullException(nameof(salt));
            if (string.IsNullOrEmpty(algorithm)) throw new ArgumentException("Algorithm cannot be empty", nameof(algorithm));

            string combined = input + salt;
            return algorithm.ToUpperInvariant() switch
            {
                "SHA256" => ComputeSha256(combined),
                "SHA512" => ComputeSha512(combined),
                "MD5" => ComputeMd5(combined),
                _ => throw new NotSupportedException($"Algorithm '{algorithm}' not supported")
            };
        }

        /// <summary>Проверяет пароль с солью.</summary>
        /// <param name="input">Пароль.</param>
        /// <param name="salt">Соль.</param>
        /// <param name="expectedHash">Ожидаемый хеш.</param>
        /// <param name="algorithm">Алгоритм.</param>
        /// <returns>true, если хеш совпадает.</returns>
        public bool VerifyWithSalt(string input, string salt, string expectedHash, string algorithm)
        {
            string actualHash = HashWithSalt(input, salt, algorithm);
            return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Добавляет запись в журнал операций.</summary>
        /// <param name="id">Идентификатор операции.</param>
        /// <param name="operation">Название операции.</param>
        /// <param name="algorithm">Алгоритм.</param>
        /// <param name="preview">Превью входных данных.</param>
        /// <param name="resultHash">Результат хеширования (может быть null).</param>
        /// <param name="success">Успех операции.</param>
        /// <param name="error">Сообщение об ошибке (необязательно).</param>
        public void LogOperation(string id, string operation, string algorithm, string preview, string? resultHash, bool success, string? error = null)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrEmpty(operation)) throw new ArgumentNullException(nameof(operation));
            var log = new HashLog(id, operation, algorithm, preview ?? "")
            {
                ResultHash = resultHash,
                Success = success,
                ErrorMessage = error
            };
            _logs.Add(log);
        }

        /// <summary>Возвращает все записи журнала.</summary>
        public List<HashLog> GetLogs() => _logs;
    }
}