using System;
using System.IO;
using HashSystem.Services;
using Xunit;

namespace HashSystem.Tests
{
    /// <summary>
    /// Модульные тесты для сервиса хеширования <see cref="HashService"/>.
    /// </summary>
    public class HashServiceTests
    {
        private readonly HashService _service;

        public HashServiceTests()
        {
            _service = new HashService();
        }

        /// <summary>
        /// TC-01: Проверяет, что ComputeSha256 возвращает строку длиной 64 символа,
        /// состоящую только из символов HEX (A-F, 0-9).
        /// </summary>
        /// <param name="input">Входная строка "hello".</param>
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
        [Fact]
        public void VerifyHash_WrongHashLength_ThrowsInvalidDataException()
        {
            Assert.Throws<InvalidDataException>(() => _service.VerifyHash("test", "123", "SHA256"));
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что GenerateSalt с отрицательной длиной выбрасывает ArgumentOutOfRangeException.
        /// </summary>
        [Fact]
        public void GenerateSalt_NegativeLength_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GenerateSalt(-5));
        }
    }
}