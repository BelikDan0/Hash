using System;

namespace HashSystem.Models
{
    /// <summary>
    /// Представляет запись о зарегистрированном файле для контроля целостности.
    /// </summary>
    public class FileRecord
    {
        /// <summary>Полный путь к файлу.</summary>
        public string FilePath { get; set; }

        /// <summary>Оригинальный хеш файла (HEX строка).</summary>
        public string OriginalHash { get; set; }

        /// <summary>Название алгоритма хеширования (SHA256, MD5).</summary>
        public string Algorithm { get; set; }

        /// <summary>Размер файла в байтах.</summary>
        public long FileSize { get; set; }

        /// <summary>Дата и время регистрации файла.</summary>
        public DateTime RegisteredAt { get; set; }

        /// <summary>Дата и время последней проверки целостности (может быть null).</summary>
        public DateTime? LastCheckedAt { get; set; }

        /// <summary>
        /// Инициализирует новую запись о файле.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <param name="originalHash">Оригинальный хеш.</param>
        /// <param name="algorithm">Алгоритм хеширования.</param>
        /// <param name="fileSize">Размер файла.</param>
        public FileRecord(string filePath, string originalHash, string algorithm, long fileSize)
        {
            FilePath = filePath;
            OriginalHash = originalHash;
            Algorithm = algorithm;
            FileSize = fileSize;
            RegisteredAt = DateTime.Now;
        }

        /// <summary>Возвращает краткое строковое представление записи.</summary>
        public override string ToString()
        {
            return $"[{FilePath}] {Algorithm}: {OriginalHash[..8]}... размер: {FileSize} байт";
        }
    }
}