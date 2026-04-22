#nullable disable
using System;
using System.IO;
using HashSystem.Models;
using HashSystem.Services;

namespace HashSystem
{
    internal class Program
    {
        /// <summary>Точка входа в консольное приложение. Демонстрирует работу сервисов.</summary>
        static void Main(string[] args)
        {
            // Создаём папку Data заранее (исправление ошибки)
            Directory.CreateDirectory("Data");

            var hashService = new HashService();
            var userService = new UserService(hashService);
            var integrityService = new FileIntegrityService(hashService);
            var storageService = new StorageService();

            Console.WriteLine("=== Хеширование строк ===");
            string text = "Hello, World!";
            string sha256 = hashService.ComputeSha256(text);
            string sha512 = hashService.ComputeSha512(text);
            string md5 = hashService.ComputeMd5(text);
            Console.WriteLine($"SHA-256 : {sha256}");
            Console.WriteLine($"SHA-512 : {sha512}");
            Console.WriteLine($"MD5     : {md5}");

            Console.WriteLine("\n=== HMAC-SHA256 ===");
            string hmac = hashService.ComputeHmacSha256(text, "secretKey");
            Console.WriteLine($"HMAC    : {hmac}");

            Console.WriteLine("\n=== Верификация хеша ===");
            bool valid = hashService.VerifyHash(text, sha256, "SHA256");
            Console.WriteLine($"Верификация SHA-256: {valid}");

            Console.WriteLine("\n=== Соль и хеш пароля ===");
            string salt = hashService.GenerateSalt();
            string salted = hashService.HashWithSalt("myPassword123", salt, "SHA256");
            bool verified = hashService.VerifyWithSalt("myPassword123", salt, salted, "SHA256");
            Console.WriteLine($"Соль    : {salt}");
            Console.WriteLine($"Хеш     : {salted}");
            Console.WriteLine($"Проверка: {verified}");

            Console.WriteLine("\n=== Регистрация пользователей ===");
            userService.RegisterUser("alice", "P@ssw0rd!");
            userService.RegisterUser("bob", "Secure#99");

            Console.WriteLine("\n=== Верификация паролей ===");
            Console.WriteLine($"alice верный пароль  : {userService.VerifyPassword("alice", "P@ssw0rd!")}");
            Console.WriteLine($"alice неверный пароль: {userService.VerifyPassword("alice", "wrongpass")}");

            Console.WriteLine("\n=== Заблокированные аккаунты ===");
            Console.WriteLine($"Заблокировано: {userService.CountLockedUsers()}");

            Console.WriteLine("\n=== Целостность файлов ===");
            File.WriteAllText("test.txt", "Данные для проверки");
            var record = integrityService.RegisterFile("test.txt", "SHA256");
            Console.WriteLine($"Зарегистрирован: {record}");
            bool intact = integrityService.VerifyFile("test.txt");
            Console.WriteLine($"Файл не изменён: {intact}");

            File.WriteAllText("test.txt", "Данные были изменены!");
            bool tampered;
            try
            {
                tampered = integrityService.VerifyFile("test.txt");
            }
            catch (DataMisalignedException)
            {
                tampered = false;
            }
            Console.WriteLine($"После изменения: {tampered}");

            Console.WriteLine("\n=== Сохранение данных ===");
            storageService.SaveCredentials("Data/users.txt", userService.GetAll());
            storageService.SaveFileRecords("Data/records.txt", integrityService.GetAll());
            hashService.LogOperation("demo", "Тест", "SHA256", text, sha256, true);
            storageService.SaveLogs("Data/logs.txt", hashService.GetLogs());
            Console.WriteLine("Данные сохранены.");

            Console.WriteLine("\n=== Загрузка данных ===");
            var loadedUsers = storageService.LoadCredentials("Data/users.txt");
            var loadedRecords = storageService.LoadFileRecords("Data/records.txt");
            Console.WriteLine($"Загружено пользователей : {loadedUsers.Count}");
            Console.WriteLine($"Загружено записей файлов: {loadedRecords.Count}");

            userService.LoadUsers(loadedUsers);
            integrityService.LoadRecords(loadedRecords);

            Console.WriteLine("\n=== Журнал операций ===");
            foreach (var log in hashService.GetLogs())
                Console.WriteLine(log);

            Console.WriteLine("\nГотово.");
        }
    }
}