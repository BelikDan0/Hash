using System;

namespace HashSystem.Models
{
    /// <summary>
    /// Учётная запись пользователя с солью и хешем пароля.
    /// </summary>
    public class UserCredential
    {
        private const int MaxFailedAttempts = 5;

        /// <summary>Имя пользователя.</summary>
        public string Username { get; set; }

        /// <summary>Хеш пароля (HEX).</summary>
        public string PasswordHash { get; set; }

        /// <summary>Соль в формате Base64.</summary>
        public string Salt { get; set; }

        /// <summary>Алгоритм хеширования пароля.</summary>
        public string Algorithm { get; set; }

        /// <summary>Дата создания учётной записи.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Дата последнего успешного входа (может быть null).</summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>Количество неудачных попыток входа подряд.</summary>
        public int FailedAttempts { get; set; }

        /// <summary>Флаг блокировки учётной записи.</summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Создаёт новую учётную запись.
        /// </summary>
        /// <param name="username">Имя пользователя.</param>
        /// <param name="passwordHash">Хеш пароля.</param>
        /// <param name="salt">Соль.</param>
        /// <param name="algorithm">Алгоритм хеширования.</param>
        public UserCredential(string username, string passwordHash, string salt, string algorithm)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
            Salt = salt ?? throw new ArgumentNullException(nameof(salt));
            Algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
            CreatedAt = DateTime.Now;
            FailedAttempts = 0;
            IsLocked = false;
        }

        /// <summary>Увеличивает счётчик неудачных попыток. При достижении лимита блокирует аккаунт.</summary>
        public void RegisterFailedAttempt()
        {
            FailedAttempts++;
            if (FailedAttempts >= MaxFailedAttempts)
                IsLocked = true;
        }

        /// <summary>Сбрасывает счётчик неудач и разблокирует аккаунт.</summary>
        public void ResetFailedAttempts()
        {
            FailedAttempts = 0;
            IsLocked = false;
        }

        /// <summary>Краткое строковое представление учётной записи.</summary>
        public override string ToString()
        {
            return $"[{Username}] alg: {Algorithm}, locked: {IsLocked}, attempts: {FailedAttempts}";
        }
    }
}