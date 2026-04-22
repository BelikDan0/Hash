#nullable disable
using System;
using System.IO;
using System.Data;
using HashSystem.Services;
using Xunit;

namespace TestProject1
{
    /// <summary>
    /// Модульные тесты для сервиса контроля целостности файлов <see cref="FileIntegrityService"/>.
    /// </summary>
    public class FileIntegrityServiceTests : IDisposable
    {
        private readonly HashService _hashService;
        private readonly FileIntegrityService _integrityService;
        private string _testFile;

        /// <summary>
        /// Инициализирует новый экземпляр тестового класса.
        /// Создаёт экземпляры <see cref="HashService"/> и <see cref="FileIntegrityService"/>.
        /// </summary>
        public FileIntegrityServiceTests()
        {
            _hashService = new HashService();
            _integrityService = new FileIntegrityService(_hashService);
        }

        /// <summary>
        /// Освобождает ресурсы: удаляет временный файл, если он был создан.
        /// </summary>
        public void Dispose()
        {
            if (_testFile != null && File.Exists(_testFile))
                File.Delete(_testFile);
        }

        /// <summary>
        /// Вспомогательный метод: создаёт временный файл с заданным содержимым.
        /// </summary>
        /// <param name="content">Содержимое файла.</param>
        private void CreateTestFile(string content)
        {
            _testFile = Path.GetTempFileName();
            File.WriteAllText(_testFile, content);
        }

        /// <summary>
        /// TC-12: Проверяет, что регистрация существующего файла создаёт запись с корректным хешем (длина 64 для SHA256).
        /// </summary>
        /// <remarks>
        /// Предусловия: временный файл с содержимым "content".
        /// Действие: вызов RegisterFile с алгоритмом SHA256.
        /// Ожидаемый результат: запись не null, путь совпадает, длина хеша равна 64.
        /// </remarks>
        [Fact]
        public void RegisterFile_ExistingFile_RecordAdded()
        {
            CreateTestFile("content");
            var record = _integrityService.RegisterFile(_testFile, "SHA256");
            Assert.NotNull(record);
            Assert.Equal(_testFile, record.FilePath);
            Assert.Equal(64, record.OriginalHash.Length);
        }

        /// <summary>
        /// TC-13: Проверяет, что для неизменённого файла VerifyFile возвращает true.
        /// </summary>
        /// <remarks>
        /// Предусловия: файл зарегистрирован, содержимое не менялось.
        /// Действие: вызов VerifyFile.
        /// Ожидаемый результат: true.
        /// </remarks>
        [Fact]
        public void VerifyFile_UnchangedFile_ReturnsTrue()
        {
            CreateTestFile("original");
            _integrityService.RegisterFile(_testFile, "SHA256");
            bool result = _integrityService.VerifyFile(_testFile);
            Assert.True(result);
        }

        /// <summary>
        /// TC-14: Проверяет, что для изменённого файла VerifyFile выбрасывает DataMisalignedException.
        /// </summary>
        /// <remarks>
        /// Предусловия: файл зарегистрирован, затем содержимое изменено.
        /// Действие: вызов VerifyFile.
        /// Ожидаемый результат: исключение DataMisalignedException.
        /// </remarks>
        /// <exception cref="DataMisalignedException">Ожидается при изменении файла.</exception>
        [Fact]
        public void VerifyFile_ChangedFile_ThrowsDataMisalignedException()
        {
            CreateTestFile("original");
            _integrityService.RegisterFile(_testFile, "SHA256");
            File.WriteAllText(_testFile, "changed");
            Assert.Throws<DataMisalignedException>(() => _integrityService.VerifyFile(_testFile));
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что вызов VerifyFile для незарегистрированного файла вызывает InvalidOperationException.
        /// </summary>
        /// <exception cref="InvalidOperationException">Ожидается, так как файл не зарегистрирован.</exception>
        [Fact]
        public void VerifyFile_NotRegistered_ThrowsInvalidOperationException()
        {
            CreateTestFile("data");
            Assert.Throws<InvalidOperationException>(() => _integrityService.VerifyFile(_testFile));
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что RegisterFile для несуществующего файла выбрасывает FileNotFoundException.
        /// </summary>
        /// <exception cref="FileNotFoundException">Ожидается, так как файл не существует.</exception>
        [Fact]
        public void RegisterFile_FileNotExist_ThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => _integrityService.RegisterFile("nonexistent.txt"));
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что после успешной проверки файла поле LastCheckedAt обновляется.
        /// </summary>
        [Fact]
        public void LoadRecords_ThenVerify_UpdatesLastCheckedAt()
        {
            CreateTestFile("data");
            var record = _integrityService.RegisterFile(_testFile, "SHA256");
            Assert.Null(record.LastCheckedAt);
            _integrityService.VerifyFile(_testFile);
            Assert.NotNull(record.LastCheckedAt);
        }

        /// <summary>
        /// Проверяет, что GetTamperedFiles возвращает список изменённых файлов.
        /// </summary>
        [Fact]
        public void GetTamperedFiles_ReturnsModifiedFiles()
        {
            CreateTestFile("original1");
            _integrityService.RegisterFile(_testFile, "SHA256");
            string file2 = Path.GetTempFileName();
            File.WriteAllText(file2, "original2");
            _integrityService.RegisterFile(file2, "SHA256");
            File.WriteAllText(_testFile, "changed");
            var tampered = _integrityService.GetTamperedFiles();
            Assert.Contains(tampered, r => r.FilePath == _testFile);
            Assert.DoesNotContain(tampered, r => r.FilePath == file2);
            File.Delete(file2);
        }

        /// <summary>
        /// Проверяет, что VerifyFile работает с алгоритмом MD5.
        /// </summary>
        [Fact]
        public void VerifyFile_WithMD5_Works()
        {
            CreateTestFile("test data");
            _integrityService.RegisterFile(_testFile, "MD5");
            bool result = _integrityService.VerifyFile(_testFile);
            Assert.True(result);
        }

        /// <summary>
        /// Проверяет, что RegisterFile с MD5 работает.
        /// </summary>
        [Fact]
        public void RegisterFile_WithMD5_Works()
        {
            CreateTestFile("data");
            var record = _integrityService.RegisterFile(_testFile, "MD5");
            Assert.Equal(32, record.OriginalHash.Length);
        }

        /// <summary>
        /// Проверяет, что ComputeFileHash с неподдерживаемым алгоритмом вызывает NotSupportedException.
        /// </summary>
        /// <exception cref="NotSupportedException">Ожидается при использовании неподдерживаемого алгоритма.</exception>
        [Fact]
        public void ComputeFileHash_UnsupportedAlgorithm_ThrowsNotSupportedException()
        {
            CreateTestFile("data");
            Assert.Throws<NotSupportedException>(() => _integrityService.RegisterFile(_testFile, "UNKNOWN"));
        }

        /// <summary>
        /// Проверяет, что GetTamperedFiles добавляет файл, если он был удалён после регистрации.
        /// </summary>
        [Fact]
        public void GetTamperedFiles_WhenFileDeleted_ReturnsAsTampered()
        {
            CreateTestFile("data");
            _integrityService.RegisterFile(_testFile, "SHA256");
            File.Delete(_testFile);
            var tampered = _integrityService.GetTamperedFiles();
            Assert.Contains(tampered, r => r.FilePath == _testFile);
        }

        /// <summary>
        /// Проверяет, что RegisterFile с пустым путём вызывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при пустом пути.</exception>
        [Fact]
        public void RegisterFile_EmptyPath_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _integrityService.RegisterFile(""));
        }

        /// <summary>
        /// Проверяет, что VerifyFile с пустым путём вызывает ArgumentNullException.
        /// </summary>
        /// <exception cref="ArgumentNullException">Ожидается при пустом пути.</exception>
        [Fact]
        public void VerifyFile_EmptyPath_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _integrityService.VerifyFile(""));
        }
        /// <summary>
        /// Проверяет, что VerifyFile выбрасывает IOException при ошибке чтения файла (файл заблокирован).
        /// </summary>
        /// <exception cref="IOException">Ожидается при блокировке файла.</exception>
        [Fact]
        public void VerifyFile_WhenFileLocked_ThrowsIOException()
        {
            CreateTestFile("data");
            _integrityService.RegisterFile(_testFile, "SHA256");
            using (var fs = new FileStream(_testFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                // Файл заблокирован, при попытке чтения в VerifyFile возникнет IOException
                Assert.Throws<IOException>(() => _integrityService.VerifyFile(_testFile));
            }
        }

        /// <summary>
        /// Проверяет, что RegisterFile выбрасывает IOException при ошибке чтения файла (файл заблокирован).
        /// </summary>
        /// <exception cref="IOException">Ожидается при блокировке файла.</exception>
        [Fact]
        public void RegisterFile_WhenFileLocked_ThrowsIOException()
        {
            CreateTestFile("data");
            using (var fs = new FileStream(_testFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                Assert.Throws<IOException>(() => _integrityService.RegisterFile(_testFile, "SHA256"));
            }
        }
        /// <summary>
        /// Проверяет, что GetTamperedFiles возвращает пустой список, когда все файлы не изменены.
        /// Покрывает ветку, где VerifyFile не выбрасывает исключение и возвращает true.
        /// </summary>
        [Fact]
        public void GetTamperedFiles_AllFilesIntact_ReturnsEmpty()
        {
            CreateTestFile("data1");
            _integrityService.RegisterFile(_testFile, "SHA256");

            string file2 = Path.GetTempFileName();
            File.WriteAllText(file2, "data2");
            _integrityService.RegisterFile(file2, "SHA256");

            var tampered = _integrityService.GetTamperedFiles();
            Assert.Empty(tampered);

            File.Delete(file2);
        }

    }
}