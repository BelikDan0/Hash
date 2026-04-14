using System;
using System.IO;
using System.Collections.Generic;
using HashSystem.Models;
using HashSystem.Services;
using Xunit;

namespace TestProject1
{
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

        // TC-15: Неверный путь (недопустимые символы) -> IOException
        [Fact]
        public void SaveCredentials_InvalidPath_ThrowsIOException()
        {
            var users = new List<UserCredential>();
            // Недопустимые символы в пути
            Assert.Throws<IOException>(() => _storage.SaveCredentials("|?*invalid", users));
        }

        // Доп. тест: сохранение и загрузка пользователей
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

        // Доп. тест: загрузка несуществующего файла -> пустой список
        [Fact]
        public void LoadCredentials_FileNotExist_ReturnsEmptyList()
        {
            var result = _storage.LoadCredentials(Path.Combine(TestDir, "nonexistent.txt"));
            Assert.Empty(result);
        }

        // Доп. тест: повреждённый файл -> FormatException
        [Fact]
        public void LoadCredentials_CorruptedFile_ThrowsFormatException()
        {
            File.WriteAllText(_testFile, "bad|data|only");
            Assert.Throws<FormatException>(() => _storage.LoadCredentials(_testFile));
        }

        // Доп. тест: сохранение и загрузка FileRecord с LastCheckedAt
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

        // Доп. тест: сохранение логов
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