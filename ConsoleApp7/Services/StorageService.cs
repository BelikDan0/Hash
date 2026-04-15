using System;
using System.Collections.Generic;
using System.IO;
using HashSystem.Models;

namespace HashSystem.Services
{
    /// <summary>
    /// Сервис для сохранения и загрузки данных (пользователи, записи файлов, журнал операций) в текстовые файлы.
    /// Формат: разделитель '|', даты в ISO 8601 (Round-trip).
    /// </summary>
    public class StorageService
    {
        /// <summary>Создаёт директорию для файла, если её нет.</summary>
        private void EnsureDirectory(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        /// <summary>Сохраняет список пользователей в файл.</summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <param name="users">Список пользователей.</param>
        /// <exception cref="ArgumentNullException">Если путь или список null.</exception>
        /// <exception cref="IOException">Ошибка записи.</exception>
        public void SaveCredentials(string filePath, List<UserCredential> users)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (users == null) throw new ArgumentNullException(nameof(users));

            EnsureDirectory(filePath);
            try
            {
                var lines = new List<string>();
                foreach (var user in users)
                {
                    lines.Add($"{user.Username}|{user.PasswordHash}|{user.Salt}|{user.Algorithm}|{user.CreatedAt:O}|{user.LastLoginAt:O}|{user.FailedAttempts}|{user.IsLocked}");
                }
                File.WriteAllLines(filePath, lines);
            }
            catch (IOException ex)
            {
                throw new IOException($"Failed to save credentials to '{filePath}'", ex);
            }
        }

        /// <summary>Загружает список пользователей из файла.</summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <returns>Список пользователей (пустой, если файл не существует).</returns>
        /// <exception cref="ArgumentNullException">Если путь null.</exception>
        /// <exception cref="FormatException">Некорректный формат данных.</exception>
        public List<UserCredential> LoadCredentials(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath)) return new List<UserCredential>();

            var users = new List<UserCredential>();
            var lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split('|');
                if (parts.Length < 8)
                    throw new FormatException($"Invalid user record at line {i + 1}: {line}");

                try
                {
                    var user = new UserCredential(parts[0], parts[1], parts[2], parts[3]);
                    if (DateTime.TryParse(parts[4], out var createdAt))
                        user.CreatedAt = createdAt;
                    if (DateTime.TryParse(parts[5], out var lastLogin))
                        user.LastLoginAt = lastLogin;
                    user.FailedAttempts = int.Parse(parts[6]);
                    user.IsLocked = bool.Parse(parts[7]);
                    users.Add(user);
                }
                catch (Exception ex)
                {
                    throw new FormatException($"Error parsing user record at line {i + 1}: {line}", ex);
                }
            }
            return users;
        }

        /// <summary>Сохраняет записи о файлах.</summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <param name="records">Список записей.</param>
        public void SaveFileRecords(string filePath, List<FileRecord> records)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (records == null) throw new ArgumentNullException(nameof(records));

            EnsureDirectory(filePath);
            try
            {
                var lines = new List<string>();
                foreach (var record in records)
                {
                    lines.Add($"{record.FilePath}|{record.OriginalHash}|{record.Algorithm}|{record.FileSize}|{record.RegisteredAt:O}|{record.LastCheckedAt:O}");
                }
                File.WriteAllLines(filePath, lines);
            }
            catch (IOException ex)
            {
                throw new IOException($"Failed to save file records to '{filePath}'", ex);
            }
        }

        /// <summary>Загружает записи о файлах.</summary>
        public List<FileRecord> LoadFileRecords(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath)) return new List<FileRecord>();

            var records = new List<FileRecord>();
            var lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split('|');
                if (parts.Length < 6)
                    throw new FormatException($"Invalid file record at line {i + 1}: {line}");

                try
                {
                    var record = new FileRecord(parts[0], parts[1], parts[2], long.Parse(parts[3]));
                    if (DateTime.TryParse(parts[4], out var reg))
                        record.RegisteredAt = reg;
                    if (DateTime.TryParse(parts[5], out var last))
                        record.LastCheckedAt = last;
                    records.Add(record);
                }
                catch (Exception ex)
                {
                    throw new FormatException($"Error parsing file record at line {i + 1}: {line}", ex);
                }
            }
            return records;
        }

        /// <summary>Сохраняет журнал операций.</summary>
        public void SaveLogs(string filePath, List<HashLog> logs)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (logs == null) throw new ArgumentNullException(nameof(logs));

            EnsureDirectory(filePath);
            try
            {
                var lines = new List<string>();
                foreach (var log in logs)
                {
                    lines.Add($"{log.Id}|{log.Operation}|{log.Algorithm}|{log.Success}|{log.Timestamp:O}|{log.ResultHash}|{log.ErrorMessage}");
                }
                File.WriteAllLines(filePath, lines);
            }
            catch (IOException ex)
            {
                throw new IOException($"Failed to save logs to '{filePath}'", ex);
            }
        }
    }
}