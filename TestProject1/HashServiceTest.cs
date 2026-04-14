using System;
using System.IO;
using HashSystem.Services;
using Xunit;

namespace HashSystem.Tests
{
    public class HashServiceTests
    {
        private readonly HashService _service;

        public HashServiceTests()
        {
            _service = new HashService();
        }

        [Fact]
        public void ComputeSha256_ValidInput_Returns64CharHex()
        {
            string hash = _service.ComputeSha256("hello");
            Assert.Equal(64, hash.Length);
            Assert.Matches("^[A-F0-9]+$", hash);
        }

        [Fact]
        public void ComputeSha256_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.ComputeSha256((string)null));
        }

        [Fact]
        public void ComputeMd5_ValidInput_Returns32CharHex()
        {
            string hash = _service.ComputeMd5("test");
            Assert.Equal(32, hash.Length);
        }

        [Fact]
        public void ComputeHmacSha256_ValidKeyAndMessage_Returns64CharHex()
        {
            string hmac = _service.ComputeHmacSha256("message", "key");
            Assert.Equal(64, hmac.Length);
        }

        [Fact]
        public void ComputeHmacSha256_EmptyKey_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.ComputeHmacSha256("msg", ""));
        }

        [Fact]
        public void VerifyHash_ValidMatch_ReturnsTrue()
        {
            string input = "hello";
            string hash = _service.ComputeSha256(input);
            bool result = _service.VerifyHash(input, hash, "SHA256");
            Assert.True(result);
        }

        [Fact]
        public void VerifyHash_InvalidMatch_ReturnsFalse()
        {
            // Хеш правильной длины (64 символа), но неверный
            string wrongHash = new string('0', 64);
            bool result = _service.VerifyHash("hello", wrongHash, "SHA256");
            Assert.False(result);
        }

        [Fact]
        public void GenerateSalt_Length16_ReturnsNonEmptyBase64()
        {
            string salt = _service.GenerateSalt(16);
            Assert.False(string.IsNullOrEmpty(salt));
            // Base64 не содержит нулевых символов, проверка на \0 избыточна
        }

        [Fact]
        public void VerifyHash_UnsupportedAlgorithm_ThrowsNotSupportedException()
        {
            Assert.Throws<NotSupportedException>(() => _service.VerifyHash("test", "hash", "UNKNOWN"));
        }

        [Fact]
        public void HashWithSalt_ValidInput_ReturnsDifferentHashesForDifferentSalts()
        {
            string hash1 = _service.HashWithSalt("pass", "salt1", "SHA256");
            string hash2 = _service.HashWithSalt("pass", "salt2", "SHA256");
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void VerifyHash_WrongHashLength_ThrowsInvalidDataException()
        {
            Assert.Throws<InvalidDataException>(() => _service.VerifyHash("test", "123", "SHA256"));
        }

        [Fact]
        public void GenerateSalt_NegativeLength_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GenerateSalt(-5));
        }
    }
}