using System;
using System.Collections.Generic;
using HashSystem.Models;

namespace HashSystem.Services
{
    /// <summary>
    /// Сервис для управления пользователями: регистрация, верификация пароля, смена пароля, блокировка.
    /// </summary>
    public class UserService
    {
        private List<UserCredential> _users;
        private readonly HashService _hashService;
        private const string DefaultAlgorithm = "SHA256";

        public UserService(HashService hashService)
        {
            _users = new List<UserCredential>();
            _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
        }

        /// <summary>Загружает список пользователей (например, из файла).</summary>
        public void LoadUsers(List<UserCredential> users)
        {
            if (users == null) throw new ArgumentNullException(nameof(users));
            _users = users;
        }

        /// <summary>Регистрирует нового пользователя.</summary>
        /// <param name="username">Имя пользователя.</param>
        /// <param name="password">Пароль.</param>
        /// <exception cref="ArgumentNullException">Если имя или пароль пусты.</exception>
        /// <exception cref="InvalidOperationException">Пользователь уже существует или ошибка регистрации.</exception>
        public void RegisterUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));

            if (_users.Find(u => u.Username == username) != null)
                throw new InvalidOperationException($"User '{username}' already exists.");

            try
            {
                string salt = _hashService.GenerateSalt();
                string hash = _hashService.HashWithSalt(password, salt, DefaultAlgorithm);
                var user = new UserCredential(username, hash, salt, DefaultAlgorithm);
                _users.Add(user);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to register user", ex);
            }
        }

        /// <summary>Проверяет пароль пользователя. Увеличивает счётчик неудачных попыток. При 5 неудачах блокирует.</summary>
        /// <param name="username">Имя пользователя.</param>
        /// <param name="password">Пароль.</param>
        /// <returns>true, если пароль верен.</returns>
        /// <exception cref="ArgumentNullException">Если имя или пароль пусты.</exception>
        /// <exception cref="InvalidOperationException">Пользователь не найден, аккаунт заблокирован или блокировка произошла.</exception>
        public bool VerifyPassword(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));

            var user = _users.Find(u => u.Username == username);
            if (user == null)
                throw new InvalidOperationException($"User '{username}' not found.");
            if (user.IsLocked)
                throw new InvalidOperationException($"Account '{username}' is locked.");

            bool isValid = _hashService.VerifyWithSalt(password, user.Salt, user.PasswordHash, user.Algorithm);
            if (!isValid)
            {
                user.RegisterFailedAttempt();
                if (user.IsLocked)
                    throw new InvalidOperationException($"Account '{username}' has been locked due to too many failed attempts.");
                return false;
            }
            else
            {
                user.ResetFailedAttempts();
                user.LastLoginAt = DateTime.Now;
                return true;
            }
        }

        /// <summary>Изменяет пароль пользователя после проверки старого пароля.</summary>
        /// <exception cref="ArgumentNullException">Если любой из параметров пуст.</exception>
        /// <exception cref="InvalidOperationException">Неверный старый пароль.</exception>
        public void ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrWhiteSpace(oldPassword)) throw new ArgumentNullException(nameof(oldPassword));
            if (string.IsNullOrWhiteSpace(newPassword)) throw new ArgumentNullException(nameof(newPassword));

            if (!VerifyPassword(username, oldPassword))
                throw new InvalidOperationException("Invalid current password.");

            var user = _users.Find(u => u.Username == username);
            string salt = _hashService.GenerateSalt();
            string hash = _hashService.HashWithSalt(newPassword, salt, DefaultAlgorithm);
            user.PasswordHash = hash;
            user.Salt = salt;
        }

        /// <summary>Возвращает пользователя по имени или null, если не найден.</summary>
        public UserCredential GetUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));
            return _users.Find(u => u.Username == username);
        }

        /// <summary>Возвращает количество заблокированных пользователей.</summary>
        public int CountLockedUsers()
        {
            int count = 0;
            foreach (var user in _users)
                if (user.IsLocked) count++;
            return count;
        }

        /// <summary>Возвращает всех пользователей.</summary>
        public List<UserCredential> GetAll() => _users;
    }
}