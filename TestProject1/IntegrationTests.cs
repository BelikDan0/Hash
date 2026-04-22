
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

        /// <summary>
        /// Инициализирует новый экземпляр тестового класса.
        /// Создаёт уникальную временную папку для изоляции теста и экземпляры всех сервисов.
        /// </summary>
        public IntegrationTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
            _hashService = new HashService();
            _userService = new UserService(_hashService);
            _fileService = new FileIntegrityService(_hashService);
            _storageService = new StorageService();
        }

        /// <summary>
        /// Освобождает ресурсы: удаляет временную папку и все её содержимое.
        /// </summary>
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
        /// <exception cref="DataMisalignedException">Ожидается при проверке изменённого файла.</exception>
        [Fact]
        public void FullWorkflow_RegisterUserAndFile_CheckIntegrity()
        {
            string userFile = Path.Combine(_tempDir, "users.txt");
            string recordsFile = Path.Combine(_tempDir, "records.txt");
            string testFile = Path.Combine(_tempDir, "test.txt");

            _userService.RegisterUser("alice", "pass123");
            Assert.True(_userService.VerifyPassword("alice", "pass123"));

            File.WriteAllText(testFile, "hello world");
            var record = _fileService.RegisterFile(testFile, "SHA256");
            Assert.True(_fileService.VerifyFile(testFile));

            File.WriteAllText(testFile, "changed");
            Assert.Throws<System.DataMisalignedException>(() => _fileService.VerifyFile(testFile));

            _storageService.SaveCredentials(userFile, _userService.GetAll());
            _storageService.SaveFileRecords(recordsFile, _fileService.GetAll());

            var loadedUsers = _storageService.LoadCredentials(userFile);
            var loadedRecords = _storageService.LoadFileRecords(recordsFile);
            Assert.Single(loadedUsers);
            Assert.Single(loadedRecords);
        }
    }
}