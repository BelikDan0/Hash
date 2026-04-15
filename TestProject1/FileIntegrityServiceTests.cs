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

        public FileIntegrityServiceTests()
        {
            _hashService = new HashService();
            _integrityService = new FileIntegrityService(_hashService);
        }

        public void Dispose()
        {
            if (_testFile != null && File.Exists(_testFile))
                File.Delete(_testFile);
        }

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
        [Fact]
        public void VerifyFile_NotRegistered_ThrowsInvalidOperationException()
        {
            CreateTestFile("data");
            Assert.Throws<InvalidOperationException>(() => _integrityService.VerifyFile(_testFile));
        }

        /// <summary>
        /// Дополнительный тест: проверяет, что RegisterFile для несуществующего файла выбрасывает FileNotFoundException.
        /// </summary>
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
    }
}