using System;
using System.IO;
using HashSystem.Services;
using Xunit;

namespace TestProject1
{
    public class IntegrationTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly HashService _hashService;
        private readonly UserService _userService;
        private readonly FileIntegrityService _fileService;
        private readonly StorageService _storageService;

        public IntegrationTests()
        {
            // Уникальная временная папка для каждого запуска теста
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);

            _hashService = new HashService();
            _userService = new UserService(_hashService);
            _fileService = new FileIntegrityService(_hashService);
            _storageService = new StorageService();
        }

        public void Dispose()
        {
            // Удаляем временную папку после теста
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        [Fact]
        public void FullWorkflow_RegisterUserAndFile_CheckIntegrity()
        {
            // Пути к файлам внутри временной папки
            string userFile = Path.Combine(_tempDir, "users.txt");
            string recordsFile = Path.Combine(_tempDir, "records.txt");
            string testFile = Path.Combine(_tempDir, "test.txt");

            // 1. Регистрируем пользователя
            _userService.RegisterUser("alice", "pass123");
            Assert.True(_userService.VerifyPassword("alice", "pass123"));

            // 2. Создаём файл и регистрируем его целостность
            File.WriteAllText(testFile, "hello world");
            var record = _fileService.RegisterFile(testFile, "SHA256");
            Assert.True(_fileService.VerifyFile(testFile));

            // 3. Изменяем файл – должно выбросить исключение
            File.WriteAllText(testFile, "changed");
            Assert.Throws<System.DataMisalignedException>(() => _fileService.VerifyFile(testFile));

            // 4. Сохраняем данные
            _storageService.SaveCredentials(userFile, _userService.GetAll());
            _storageService.SaveFileRecords(recordsFile, _fileService.GetAll());

            // 5. Загружаем и проверяем
            var loadedUsers = _storageService.LoadCredentials(userFile);
            var loadedRecords = _storageService.LoadFileRecords(recordsFile);

            Assert.Single(loadedUsers);
            Assert.Single(loadedRecords);
        }
    }
}