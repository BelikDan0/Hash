using System;
using System.IO;
using System.Data;
using HashSystem.Services;
using Xunit;

namespace TestProject1
{
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

        // TC-12: Регистрация существующего файла
        [Fact]
        public void RegisterFile_ExistingFile_RecordAdded()
        {
            CreateTestFile("content");
            var record = _integrityService.RegisterFile(_testFile, "SHA256");
            Assert.NotNull(record);
            Assert.Equal(_testFile, record.FilePath);
            Assert.Equal(64, record.OriginalHash.Length);
        }

        // TC-13: Файл не изменён -> true
        [Fact]
        public void VerifyFile_UnchangedFile_ReturnsTrue()
        {
            CreateTestFile("original");
            _integrityService.RegisterFile(_testFile, "SHA256");
            bool result = _integrityService.VerifyFile(_testFile);
            Assert.True(result);
        }

        // TC-14: Файл изменён -> DataMisalignedException
        [Fact]
        public void VerifyFile_ChangedFile_ThrowsDataMisalignedException()
        {
            CreateTestFile("original");
            _integrityService.RegisterFile(_testFile, "SHA256");
            File.WriteAllText(_testFile, "changed");
            Assert.Throws<DataMisalignedException>(() => _integrityService.VerifyFile(_testFile));
        }

        // Доп. тест: файл не зарегистрирован
        [Fact]
        public void VerifyFile_NotRegistered_ThrowsInvalidOperationException()
        {
            CreateTestFile("data");
            Assert.Throws<InvalidOperationException>(() => _integrityService.VerifyFile(_testFile));
        }

        // Доп. тест: файл не существует при регистрации
        [Fact]
        public void RegisterFile_FileNotExist_ThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => _integrityService.RegisterFile("nonexistent.txt"));
        }

        // Доп. тест: загрузка записей и проверка LastCheckedAt
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