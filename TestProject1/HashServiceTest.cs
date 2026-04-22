#nullable disable
using HashSystem.Models;
using HashSystem.Services;
using System;
using System.IO;
using Xunit;

namespace HashSystem.Tests
{
    /// <summary>
    /// Модульные тесты для сервиса хеширования <see cref="HashService"/>.
    /// </summary>
    public class HashServiceTests
    {
        private readonly HashService _service;

        /// <summary>
        /// Инициализирует новый экземпляр тестового класса.
        /// Создаёт экземпляр <see cref="HashService"/>.
        /// </summary>
        public HashServiceTests()
        {
            _service = new HashService();
        }

        /// <summary>
        /// TC-01: Проверяет, что ComputeSha256 возвращает строку длиной 64 символа,
        /// состоящую только из символов HEX (A-F, 0-9).
        /// </summary>
        [Fact]
        public void ComputeSha256_ValidInput_Returns64CharHex()
        {
            string hash = _service.ComputeSha256("hello");
            Assert.Equal(64, hash.Length);
            Assert.Matches("^[A-F0-9]+$", hash);
        }

        /// <summary>
        /// TC-02: Проверяет, что передача null в ComputeSha256 вызывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при передаче null.</exception>
        [Fact]
        public void ComputeSha256_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.ComputeSha256((string)null));
        }

        /// <summary>
        /// TC-03: Проверяет, что ComputeMd5 возвращает строку длиной 32 символа.
        /// </summary>
        [Fact]
        public void ComputeMd5_ValidInput_Returns32CharHex()
        {
            string hash = _service.ComputeMd5("test");
            Assert.Equal(32, hash.Length);
        }

        /// <summary>
        /// TC-04: Проверяет, что ComputeHmacSha256 возвращает строку длиной 64 символа.
        /// </summary>
        [Fact]
        public void ComputeHmacSha256_ValidKeyAndMessage_Returns64CharHex()
        {
            string hmac = _service.ComputeHmacSha256("message", "key");
            Assert.Equal(64, hmac.Length);
        }

        /// <summary>
        /// TC-05: Проверяет, что при пустом ключе ComputeHmacSha256 выбрасывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при пустом ключе.</exception>
        [Fact]
        public void ComputeHmacSha256_EmptyKey_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.ComputeHmacSha256("msg", ""));
        }

        /// <summary>
        /// TC-06: Проверяет, что VerifyHash возвращает true для корректной пары (строка, хеш).
        /// </summary>
        [Fact]
        public void VerifyHash_ValidMatch_ReturnsTrue()
        {
            string input = "hello";
            string hash = _service.ComputeSha256(input);
            bool result = _service.VerifyHash(input, hash, "SHA256");
            Assert.True(result);
        }

        /// <summary>
        /// TC-07: Проверяет, что VerifyHash возвращает false, если передан неверный хеш (но правильной длины).
        /// </summary>
        [Fact]
        public void VerifyHash_InvalidMatch_ReturnsFalse()
        {
            string wrongHash = new string('0', 64);
            bool result = _service.VerifyHash("hello", wrongHash, "SHA256");
            Assert.False(result);
        }

        /// <summary>
        /// TC-16: Проверяет, что GenerateSalt возвращает непустую строку в формате Base64.
        /// </summary>
        [Fact]
        public void GenerateSalt_Length16_ReturnsNonEmptyBase64()
        {
            string salt = _service.GenerateSalt(16);
            Assert.False(string.IsNullOrEmpty(salt));
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что при неподдерживаемом алгоритме VerifyHash выбрасывает NotSupportedException.
        /// </summary>
        /// <exception cref="NotSupportedException">Ожидается при неизвестном алгоритме.</exception>
        [Fact]
        public void VerifyHash_UnsupportedAlgorithm_ThrowsNotSupportedException()
        {
            Assert.Throws<NotSupportedException>(() => _service.VerifyHash("test", "hash", "UNKNOWN"));
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что хеши одной и той же строки с разными солями различаются.
        /// </summary>
        [Fact]
        public void HashWithSalt_ValidInput_ReturnsDifferentHashesForDifferentSalts()
        {
            string hash1 = _service.HashWithSalt("pass", "salt1", "SHA256");
            string hash2 = _service.HashWithSalt("pass", "salt2", "SHA256");
            Assert.NotEqual(hash1, hash2);
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что при передаче хеша неверной длины VerifyHash выбрасывает InvalidDataException.
        /// </summary>
        /// <exception cref="InvalidDataException">Ожидается при неверной длине хеша.</exception>
        [Fact]
        public void VerifyHash_WrongHashLength_ThrowsInvalidDataException()
        {
            Assert.Throws<InvalidDataException>(() => _service.VerifyHash("test", "123", "SHA256"));
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что GenerateSalt с отрицательной длиной выбрасывает ArgumentOutOfRangeException.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Ожидается при отрицательной длине.</exception>
        [Fact]
        public void GenerateSalt_NegativeLength_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GenerateSalt(-5));
        }

        /// <summary>
        /// Проверяет, что ComputeSha512 для строки возвращает 128 символов HEX.
        /// </summary>
        [Fact]
        public void ComputeSha512_ValidInput_Returns128CharHex()
        {
            string hash = _service.ComputeSha512("hello");
            Assert.Equal(128, hash.Length);
            Assert.Matches("^[A-F0-9]+$", hash);
        }

        /// <summary>
        /// Проверяет, что ComputeSha512 с null вызывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при передаче null.</exception>
        [Fact]
        public void ComputeSha512_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.ComputeSha512((string)null));
        }

        /// <summary>
        /// Проверяет ComputeSha512 для массива байт.
        /// </summary>
        [Fact]
        public void ComputeSha512_Bytes_ReturnsCorrectHash()
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes("test");
            string hash = _service.ComputeSha512(data);
            Assert.Equal(128, hash.Length);
        }

        /// <summary>
        /// Проверяет ComputeMd5 для массива байт.
        /// </summary>
        [Fact]
        public void ComputeMd5_Bytes_ReturnsCorrectHash()
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes("test");
            string hash = _service.ComputeMd5(data);
            Assert.Equal(32, hash.Length);
        }

        /// <summary>
        /// Проверяет VerifyHash для SHA512 с корректным хешем.
        /// </summary>
        [Fact]
        public void VerifyHash_SHA512_ValidMatch_ReturnsTrue()
        {
            string input = "hello";
            string hash = _service.ComputeSha512(input);
            bool result = _service.VerifyHash(input, hash, "SHA512");
            Assert.True(result);
        }

        /// <summary>
        /// Проверяет VerifyHash для SHA512 с неверным хешем.
        /// </summary>
        [Fact]
        public void VerifyHash_SHA512_InvalidMatch_ReturnsFalse()
        {
            string wrongHash = new string('0', 128);
            bool result = _service.VerifyHash("hello", wrongHash, "SHA512");
            Assert.False(result);
        }

        /// <summary>
        /// Проверяет HashWithSalt для SHA512: разные соли дают разные хеши.
        /// </summary>
        [Fact]
        public void HashWithSalt_SHA512_ReturnsDifferentHashes()
        {
            string hash1 = _service.HashWithSalt("pass", "salt1", "SHA512");
            string hash2 = _service.HashWithSalt("pass", "salt2", "SHA512");
            Assert.NotEqual(hash1, hash2);
        }

        /// <summary>
        /// Проверяет HashWithSalt для MD5.
        /// </summary>
        [Fact]
        public void HashWithSalt_MD5_Works()
        {
            string hash = _service.HashWithSalt("pass", "salt", "MD5");
            Assert.Equal(32, hash.Length);
        }

        /// <summary>
        /// Проверяет VerifyWithSalt для корректных данных.
        /// </summary>
        [Fact]
        public void VerifyWithSalt_Valid_ReturnsTrue()
        {
            string salt = _service.GenerateSalt();
            string hash = _service.HashWithSalt("mypass", salt, "SHA256");
            bool result = _service.VerifyWithSalt("mypass", salt, hash, "SHA256");
            Assert.True(result);
        }

        /// <summary>
        /// Проверяет GenerateSalt с длиной по умолчанию (16).
        /// </summary>
        [Fact]
        public void GenerateSalt_DefaultLength_ReturnsNonEmpty()
        {
            string salt = _service.GenerateSalt();
            Assert.False(string.IsNullOrEmpty(salt));
        }

        /// <summary>
        /// Проверяет, что LogOperation добавляет запись в журнал.
        /// </summary>
        [Fact]
        public void LogOperation_AddsLogEntry()
        {
            var logsBefore = _service.GetLogs().Count;
            _service.LogOperation("test-id", "TestOp", "SHA256", "preview", "hash123", true);
            var logsAfter = _service.GetLogs().Count;
            Assert.Equal(logsBefore + 1, logsAfter);
        }

        /// <summary>
        /// Проверяет, что GetLogs возвращает все записи.
        /// </summary>
        [Fact]
        public void GetLogs_ReturnsAllLogs()
        {
            _service.LogOperation("id1", "Op1", "MD5", "in", "hash1", true);
            _service.LogOperation("id2", "Op2", "SHA256", "in2", "hash2", false, "error");
            var logs = _service.GetLogs();
            Assert.Contains(logs, l => l.Id == "id1");
            Assert.Contains(logs, l => l.Id == "id2");
        }

        /// <summary>
        /// Проверяет, что ComputeSha256(byte[]) с null вызывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при передаче null.</exception>
        [Fact]
        public void ComputeSha256_BytesNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.ComputeSha256((byte[])null));
        }

        /// <summary>
        /// Проверяет, что ComputeSha512(byte[]) с null вызывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при передаче null.</exception>
        [Fact]
        public void ComputeSha512_BytesNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.ComputeSha512((byte[])null));
        }

        /// <summary>
        /// Проверяет, что ComputeMd5(byte[]) с null вызывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при передаче null.</exception>
        [Fact]
        public void ComputeMd5_BytesNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.ComputeMd5((byte[])null));
        }

        /// <summary>
        /// Проверяет, что ComputeHmacSha256 с null сообщением вызывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при передаче null.</exception>
        [Fact]
        public void ComputeHmacSha256_NullMessage_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.ComputeHmacSha256(null, "key"));
        }

        /// <summary>
        /// Проверяет, что HashWithSalt с неподдерживаемым алгоритмом вызывает NotSupportedException.
        /// </summary>
        /// <exception cref="NotSupportedException">Ожидается при неизвестном алгоритме.</exception>
        [Fact]
        public void HashWithSalt_UnsupportedAlgorithm_ThrowsNotSupportedException()
        {
            Assert.Throws<NotSupportedException>(() => _service.HashWithSalt("pass", "salt", "UNKNOWN"));
        }

        /// <summary>
        /// Проверяет, что VerifyWithSalt с неверным хешем возвращает false.
        /// </summary>
        [Fact]
        public void VerifyWithSalt_InvalidHash_ReturnsFalse()
        {
            string salt = _service.GenerateSalt();
            string hash = _service.HashWithSalt("mypass", salt, "SHA256");
            bool result = _service.VerifyWithSalt("wrongpass", salt, hash, "SHA256");
            Assert.False(result);
        }

        /// <summary>
        /// Проверяет, что VerifyHash с SHA512 и неправильной длиной хеша вызывает InvalidDataException.
        /// </summary>
        /// <exception cref="InvalidDataException">Ожидается при неверной длине хеша.</exception>
        [Fact]
        public void VerifyHash_SHA512_WrongHashLength_ThrowsInvalidDataException()
        {
            Assert.Throws<InvalidDataException>(() => _service.VerifyHash("test", "123", "SHA512"));
        }

        /// <summary>
        /// Проверяет, что VerifyHash с MD5 и неправильной длиной хеша вызывает InvalidDataException.
        /// </summary>
        /// <exception cref="InvalidDataException">Ожидается при неверной длине хеша.</exception>
        [Fact]
        public void VerifyHash_MD5_WrongHashLength_ThrowsInvalidDataException()
        {
            Assert.Throws<InvalidDataException>(() => _service.VerifyHash("test", "123", "MD5"));
        }

        /// <summary>
        /// Проверяет ComputeSha512 для массива байт (HEX, 128 символов).
        /// </summary>
        [Fact]
        public void ComputeSha512_Bytes_Returns128CharHex()
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes("test");
            string hash = _service.ComputeSha512(data);
            Assert.Equal(128, hash.Length);
            Assert.Matches("^[A-F0-9]+$", hash);
        }

        /// <summary>
        /// Проверяет, что HashWithSalt с null входом выбрасывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при передаче null.</exception>
        [Fact]
        public void HashWithSalt_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.HashWithSalt(null, "salt", "SHA256"));
        }

        /// <summary>
        /// Проверяет, что HashWithSalt с пустой солью выбрасывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при пустой соли.</exception>
        [Fact]
        public void HashWithSalt_EmptySalt_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.HashWithSalt("pass", "", "SHA256"));
        }

        /// <summary>
        /// Проверяет, что GenerateSalt с максимальной длиной (1024) работает.
        /// </summary>
        [Fact]
        public void GenerateSalt_MaxLength_Works()
        {
            string salt = _service.GenerateSalt(1024);
            Assert.False(string.IsNullOrEmpty(salt));
        }

        /// <summary>
        /// Проверяет, что LogOperation с null Id выбрасывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при null Id.</exception>
        [Fact]
        public void LogOperation_NullId_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.LogOperation(null, "op", "alg", "prev", "hash", true));
        }

        /// <summary>
        /// Проверяет, что LogOperation с пустой операцией выбрасывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при пустой операции.</exception>
        [Fact]
        public void LogOperation_EmptyOperation_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.LogOperation("id", "", "alg", "prev", "hash", true));
        }

        /// <summary>
        /// Проверяет, что VerifyWithSalt с неподдерживаемым алгоритмом выбрасывает NotSupportedException.
        /// </summary>
        /// <exception cref="NotSupportedException">Ожидается при неизвестном алгоритме.</exception>
        [Fact]
        public void VerifyWithSalt_UnsupportedAlgorithm_ThrowsNotSupportedException()
        {
            string salt = _service.GenerateSalt();
            string hash = _service.HashWithSalt("pass", salt, "SHA256");
            Assert.Throws<NotSupportedException>(() => _service.VerifyWithSalt("pass", salt, hash, "UNKNOWN"));
        }
        /// <summary>
        /// Проверяет VerifyHash для MD5 (корректный и неверный).
        /// </summary>
        [Fact]
        public void VerifyHash_MD5_ValidMatch_ReturnsTrue()
        {
            string input = "hello";
            string hash = _service.ComputeMd5(input);
            Assert.True(_service.VerifyHash(input, hash, "MD5"));
        }

        [Fact]
        public void VerifyHash_MD5_InvalidMatch_ReturnsFalse()
        {
            string wrongHash = new string('0', 32);
            Assert.False(_service.VerifyHash("hello", wrongHash, "MD5"));
        }

        /// <summary>
        /// Проверяет VerifyHash с пустым алгоритмом.
        /// </summary>
        [Fact]
        public void VerifyHash_EmptyAlgorithm_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _service.VerifyHash("test", "hash", ""));
        }
        /// <summary>
        /// Проверяет ComputeHmacSha256 с пустым сообщением (должен вернуть хеш, а не исключение).
        /// </summary>
        [Fact]
        public void ComputeHmacSha256_EmptyMessage_ReturnsHash()
        {
            string hmac = _service.ComputeHmacSha256("", "key");
            Assert.Equal(64, hmac.Length);
        }

        /// <summary>
        /// Проверяет VerifyHash с алгоритмом в нижнем регистре (должен работать).
        /// </summary>
        [Fact]
        public void VerifyHash_LowercaseAlgorithm_ReturnsTrue()
        {
            string input = "hello";
            string hash = _service.ComputeSha256(input);
            bool result = _service.VerifyHash(input, hash, "sha256");
            Assert.True(result);
        }

        /// <summary>
        /// Проверяет HashWithSalt с алгоритмом в нижнем регистре.
        /// </summary>
        [Fact]
        public void HashWithSalt_LowercaseAlgorithm_Works()
        {
            string hash = _service.HashWithSalt("pass", "salt", "sha256");
            Assert.Equal(64, hash.Length);
        }

        /// <summary>
        /// Проверяет VerifyWithSalt с хешем в нижнем регистре (должен сравнивать без учёта регистра).
        /// </summary>
        [Fact]
        public void VerifyWithSalt_LowercaseHash_ReturnsTrue()
        {
            string salt = _service.GenerateSalt();
            string hash = _service.HashWithSalt("mypass", salt, "SHA256").ToLower();
            bool result = _service.VerifyWithSalt("mypass", salt, hash, "SHA256");
            Assert.True(result);
        }

        /// <summary>
        /// Проверяет LogOperation с null preview (должен заменяться на пустую строку без исключения).
        /// </summary>
        [Fact]
        public void LogOperation_NullPreview_DoesNotThrow()
        {
            var before = _service.GetLogs().Count;
            _service.LogOperation("id", "op", "alg", null, "hash", true);
            var after = _service.GetLogs().Count;
            Assert.Equal(before + 1, after);
        }

        /// <summary>
        /// Проверяет ComputeSha256 с очень длинной строкой (покрывает обрезание preview в LogError).
        /// </summary>
        [Fact]
        public void ComputeSha256_VeryLongInput_DoesNotThrow()
        {
            string longInput = new string('a', 10000);
            string hash = _service.ComputeSha256(longInput);
            Assert.Equal(64, hash.Length);
        }
        /// <summary>
        /// Проверяет метод ToString моделей (для увеличения покрытия).
        /// </summary>
        [Fact]
        public void ModelToString_Coverage()
        {
            var fileRecord = new FileRecord("path", "0123456789ABCDEF", "SHA256", 100);
            Assert.Contains("path", fileRecord.ToString());

            var user = new UserCredential("user", "hash", "salt", "SHA256");
            Assert.Contains("user", user.ToString());

            var log = new HashLog("id", "op", "alg", "preview");
            Assert.Contains("op", log.ToString());
        }
    }
}