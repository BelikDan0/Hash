#nullable disable
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

        /// <summary>
        /// Инициализирует новый экземпляр тестового класса.
        /// Создаёт временную папку TestData и определяет путь к тестовому файлу.
        /// </summary>
        public StorageServiceTests()
        {
            _storage = new StorageService();
            Directory.CreateDirectory(TestDir);
            _testFile = Path.Combine(TestDir, "test_creds.txt");
        }

        /// <summary>
        /// Освобождает ресурсы: удаляет папку TestData и всё её содержимое.
        /// </summary>
        public void Dispose()
        {
            if (Directory.Exists(TestDir))
                Directory.Delete(TestDir, true);
        }

        /// <summary>
        /// TC-15: Проверяет, что при попытке сохранить данные по пути с недопустимыми символами выбрасывается IOException.
        /// </summary>
        /// <exception cref="IOException">Ожидается при недопустимом пути.</exception>
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
        /// <exception cref="FormatException">Ожидается при повреждённом формате.</exception>
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

        /// <summary>
        /// Проверяет, что сохранение пустого списка пользователей создаёт файл (возможно пустой).
        /// </summary>
        [Fact]
        public void SaveCredentials_EmptyList_CreatesFile()
        {
            var empty = new List<UserCredential>();
            _storage.SaveCredentials(_testFile, empty);
            Assert.True(File.Exists(_testFile));
            var lines = File.ReadAllLines(_testFile);
            Assert.Empty(lines);
        }

        /// <summary>
        /// Проверяет, что загрузка из пустого файла возвращает пустой список.
        /// </summary>
        [Fact]
        public void LoadCredentials_EmptyFile_ReturnsEmptyList()
        {
            File.WriteAllText(_testFile, "");
            var result = _storage.LoadCredentials(_testFile);
            Assert.Empty(result);
        }

        /// <summary>
        /// Проверяет, что сохранение пустого списка записей файлов создаёт файл.
        /// </summary>
        [Fact]
        public void SaveFileRecords_EmptyList_CreatesFile()
        {
            var empty = new List<FileRecord>();
            var path = Path.Combine(TestDir, "empty_records.txt");
            _storage.SaveFileRecords(path, empty);
            Assert.True(File.Exists(path));
            File.Delete(path);
        }

        /// <summary>
        /// Проверяет, что загрузка из пустого файла записей возвращает пустой список.
        /// </summary>
        [Fact]
        public void LoadFileRecords_EmptyFile_ReturnsEmptyList()
        {
            var path = Path.Combine(TestDir, "empty_records_load.txt");
            File.WriteAllText(path, "");
            var result = _storage.LoadFileRecords(path);
            Assert.Empty(result);
            File.Delete(path);
        }

        /// <summary>
        /// Проверяет, что сохранение пустого списка логов создаёт файл.
        /// </summary>
        [Fact]
        public void SaveLogs_EmptyList_CreatesFile()
        {
            var empty = new List<HashLog>();
            var path = Path.Combine(TestDir, "empty_logs.txt");
            _storage.SaveLogs(path, empty);
            Assert.True(File.Exists(path));
            File.Delete(path);
        }

        /// <summary>
        /// Проверяет, что загрузка файла с отсутствующей датой LastCheckedAt не падает.
        /// </summary>
        [Fact]
        public void LoadFileRecords_MissingDate_HandlesNull()
        {
            var lines = new[] { "file.txt|hash|SHA256|100|2023-01-01T00:00:00.0000000|" };
            var path = Path.Combine(TestDir, "missing_date.txt");
            File.WriteAllLines(path, lines);
            var records = _storage.LoadFileRecords(path);
            Assert.Single(records);
            Assert.Null(records[0].LastCheckedAt);
            File.Delete(path);
        }

        /// <summary>
        /// Проверяет, что LoadFileRecords с некорректной строкой (меньше 6 полей) вызывает FormatException.
        /// </summary>
        /// <exception cref="FormatException">Ожидается при некорректном формате строки.</exception>
        [Fact]
        public void LoadFileRecords_CorruptedLine_ThrowsFormatException()
        {
            var path = Path.Combine(TestDir, "bad_records.txt");
            File.WriteAllLines(path, new[] { "file.txt|hash|SHA256" });
            Assert.Throws<FormatException>(() => _storage.LoadFileRecords(path));
            File.Delete(path);
        }

        /// <summary>
        /// Проверяет, что SaveCredentials с null списком вызывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при передаче null.</exception>
        [Fact]
        public void SaveCredentials_NullUsers_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _storage.SaveCredentials(_testFile, null));
        }

        /// <summary>
        /// Проверяет, что SaveFileRecords с null списком вызывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при передаче null.</exception>
        [Fact]
        public void SaveFileRecords_NullRecords_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _storage.SaveFileRecords(_testFile, null));
        }

        /// <summary>
        /// Проверяет, что SaveLogs с null списком вызывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при передаче null.</exception>
        [Fact]
        public void SaveLogs_NullLogs_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _storage.SaveLogs(_testFile, null));
        }

        /// <summary>
        /// Проверяет, что LoadCredentials с пустым путём вызывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при пустом пути.</exception>
        [Fact]
        public void LoadCredentials_EmptyPath_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _storage.LoadCredentials(""));
        }

        /// <summary>
        /// Проверяет, что LoadFileRecords с пустым путём вызывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при пустом пути.</exception>
        [Fact]
        public void LoadFileRecords_EmptyPath_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _storage.LoadFileRecords(""));
        }

    }
}