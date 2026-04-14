using System;

namespace HashSystem.Models
{
    public class UserCredential
    {
        private const int MaxFailedAttempts = 5;

        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public string Algorithm { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int FailedAttempts { get; set; }
        public bool IsLocked { get; set; }

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

        public void RegisterFailedAttempt()
        {
            FailedAttempts++;
            if (FailedAttempts >= MaxFailedAttempts)
                IsLocked = true;
        }

        public void ResetFailedAttempts()
        {
            FailedAttempts = 0;
            IsLocked = false;
        }

        public override string ToString()
        {
            return $"[{Username}] alg: {Algorithm}, locked: {IsLocked}, attempts: {FailedAttempts}";
        }
    }
}