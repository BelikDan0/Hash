using System;
using System.IO;
using HashSystem.Services;
using Xunit;

namespace TestProject1
{
    /// <summary>
    /// Интеграционные тесты, проверяющие совместную работу нескольких сервисов:
    /// регистрация пользователя, целостность файлов, сохранение и загрузка данных.
    /// </summary>
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
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        /// <summary>
        /// Полный интеграционный тест, проверяющий сквозной сценарий:
        /// 1. Регистрация пользователя и проверка пароля.
        /// 2. Создание файла, регистрация его целостности и проверка.
        /// 3. Изменение файла и ожидание исключения DataMisalignedException.
        /// 4. Сохранение всех данных (пользователи, записи файлов) в файлы.
        /// 5. Загрузка данных обратно и проверка количества записей.
        /// </summary>
        [Fact]
        public void FullWorkflow_RegisterUserAndFile_CheckIntegrity()
        {
            // Пути к файлам внутри временной папки
            string userFile = Path.Combine(_tempDir, "users.txt");
            string recordsFile = Path.Combine(_tempDir, "records.txt");
            string testFile = Path.Combine(_tempDir, "test.txt");

            // Регистрация пользователя
            _userService.RegisterUser("alice", "pass123");
            Assert.True(_userService.VerifyPassword("alice", "pass123"));

            // Регистрация файла
            File.WriteAllText(testFile, "hello world");
            var record = _fileService.RegisterFile(testFile, "SHA256");
            Assert.True(_fileService.VerifyFile(testFile));

            // Изменение файла
            File.WriteAllText(testFile, "changed");
            Assert.Throws<System.DataMisalignedException>(() => _fileService.VerifyFile(testFile));

            // Сохранение
            _storageService.SaveCredentials(userFile, _userService.GetAll());
            _storageService.SaveFileRecords(recordsFile, _fileService.GetAll());

            // Загрузка и проверка
            var loadedUsers = _storageService.LoadCredentials(userFile);
            var loadedRecords = _storageService.LoadFileRecords(recordsFile);
            Assert.Single(loadedUsers);
            Assert.Single(loadedRecords);
        }
    }
}