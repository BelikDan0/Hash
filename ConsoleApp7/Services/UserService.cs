using System;
using System.Collections.Generic;
using HashSystem.Models;

namespace HashSystem.Services
{
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

        public void LoadUsers(List<UserCredential> users)
        {
            if (users == null) throw new ArgumentNullException(nameof(users));
            _users = users;
        }

        public void RegisterUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));

            var existing = _users.Find(u => u.Username == username);
            if (existing != null)
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

        public UserCredential GetUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));
            return _users.Find(u => u.Username == username);
        }

        public int CountLockedUsers()
        {
            int count = 0;
            foreach (var user in _users)
                if (user.IsLocked) count++;
            return count;
        }

        public List<UserCredential> GetAll() => _users;
    }
}