using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using HashSystem.Models;

namespace HashSystem.Services
{
    public class HashService
    {
        private List<HashLog> _logs;

        public HashService()
        {
            _logs = new List<HashLog>();
        }

        // --- Вспомогательный метод для логирования ---
        private void LogError(string operation, string algorithm, string inputPreview, Exception ex)
        {
            LogOperation(Guid.NewGuid().ToString(), operation, algorithm, inputPreview, null, false, ex.Message);
        }

        // --- Работа со строками ---
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

        public string ComputeSha512(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return ComputeSha512(bytes);
        }

        public string ComputeMd5(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return ComputeMd5(bytes);
        }

        // --- Работа с байтами ---
        public string ComputeSha256(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }

        public string ComputeSha512(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            using var sha = SHA512.Create();
            byte[] hash = sha.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }

        public string ComputeMd5(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            using var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }

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

        public bool VerifyHash(string input, string expectedHash, string algorithm)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (expectedHash == null) throw new ArgumentNullException(nameof(expectedHash));
            if (string.IsNullOrEmpty(algorithm)) throw new ArgumentException("Algorithm cannot be empty", nameof(algorithm));

            // Проверка длины хеша согласно алгоритму
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

        public string GenerateSalt(int length = 16)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length), "Salt length must be positive");
            if (length > 1024) throw new ArgumentOutOfRangeException(nameof(length), "Salt length too large (max 1024)");
            byte[] bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public string HashWithSalt(string input, string salt, string algorithm)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (string.IsNullOrEmpty(salt)) throw new ArgumentNullException(nameof(salt));
            if (string.IsNullOrEmpty(algorithm)) throw new ArgumentException("Algorithm cannot be empty", nameof(algorithm));

            string combined = input + salt; // порядок input+salt более стоек
            return algorithm.ToUpperInvariant() switch
            {
                "SHA256" => ComputeSha256(combined),
                "SHA512" => ComputeSha512(combined),
                "MD5" => ComputeMd5(combined),
                _ => throw new NotSupportedException($"Algorithm '{algorithm}' not supported")
            };
        }

        public bool VerifyWithSalt(string input, string salt, string expectedHash, string algorithm)
        {
            string actualHash = HashWithSalt(input, salt, algorithm);
            return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }

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

        public List<HashLog> GetLogs() => _logs;
    }
}