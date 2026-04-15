using System;
using System.IO;
using System.Collections.Generic;
using HashSystem.Models;
using HashSystem.Services;
using Xunit;

namespace TestProject1
{
    /// <summary>
    /// Модульные тесты для сервиса хранения данных <see cref="StorageService"/>.
    /// </summary>
    public class StorageServiceTests : IDisposable
    {
        private readonly StorageService _storage;
        private const string TestDir = "TestData";
        private readonly string _testFile;

        public StorageServiceTests()
        {
            _storage = new StorageService();
            Directory.CreateDirectory(TestDir);
            _testFile = Path.Combine(TestDir, "test_creds.txt");
        }

        public void Dispose()
        {
            if (Directory.Exists(TestDir))
                Directory.Delete(TestDir, true);
        }

        /// <summary>
        /// TC-15: Проверяет, что при попытке сохранить данные по пути с недопустимыми символами выбрасывается IOException.
        /// </summary>
        [Fact]
        public void SaveCredentials_InvalidPath_ThrowsIOException()
        {
            var users = new List<UserCredential>();
            Assert.Throws<IOException>(() => _storage.SaveCredentials("|?*invalid", users));
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что после сохранения и последующей загрузки данные пользователя остаются неизменными.
        /// </summary>
        [Fact]
        public void SaveAndLoadCredentials_Roundtrip_DataPreserved()
        {
            var original = new List<UserCredential>
            {
                new UserCredential("user1", "hash1", "salt1", "SHA256")
            };
            _storage.SaveCredentials(_testFile, original);
            var loaded = _storage.LoadCredentials(_testFile);
            Assert.Single(loaded);
            Assert.Equal("user1", loaded[0].Username);
            Assert.Equal("hash1", loaded[0].PasswordHash);
            Assert.Equal("salt1", loaded[0].Salt);
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что загрузка из несуществующего файла возвращает пустой список, а не выбрасывает исключение.
        /// </summary>
        [Fact]
        public void LoadCredentials_FileNotExist_ReturnsEmptyList()
        {
            var result = _storage.LoadCredentials(Path.Combine(TestDir, "nonexistent.txt"));
            Assert.Empty(result);
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что при загрузке повреждённого файла (неверный формат) выбрасывается FormatException.
        /// </summary>
        [Fact]
        public void LoadCredentials_CorruptedFile_ThrowsFormatException()
        {
            File.WriteAllText(_testFile, "bad|data|only");
            Assert.Throws<FormatException>(() => _storage.LoadCredentials(_testFile));
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что при сохранении и загрузке записей о файлах поле LastCheckedAt не теряется.
        /// </summary>
        [Fact]
        public void SaveAndLoadFileRecords_Roundtrip_PreservesLastCheckedAt()
        {
            var record = new FileRecord("test.txt", "hash", "SHA256", 100);
            record.LastCheckedAt = DateTime.Now;
            var list = new List<FileRecord> { record };
            var filePath = Path.Combine(TestDir, "records.txt");
            _storage.SaveFileRecords(filePath, list);
            var loaded = _storage.LoadFileRecords(filePath);
            Assert.Single(loaded);
            Assert.NotNull(loaded[0].LastCheckedAt);
            Assert.Equal(record.LastCheckedAt.Value.ToString("O"), loaded[0].LastCheckedAt.Value.ToString("O"));
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что сохранение логов создаёт файл и он не пуст.
        /// </summary>
        [Fact]
        public void SaveLogs_ValidLogs_FileCreated()
        {
            var logs = new List<HashLog>
            {
                new HashLog("1", "Test", "SHA256", "preview") { Success = true, ResultHash = "abc" }
            };
            var logPath = Path.Combine(TestDir, "logs.txt");
            _storage.SaveLogs(logPath, logs);
            Assert.True(File.Exists(logPath));
            var lines = File.ReadAllLines(logPath);
            Assert.NotEmpty(lines);
        }
    }
}