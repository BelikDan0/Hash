using System;
using System.Collections.Generic;
using System.IO;
using HashSystem.Models;

namespace HashSystem.Services
{
    public class FileIntegrityService
    {
        private List<FileRecord> _records;
        private readonly HashService _hashService;

        public FileIntegrityService(HashService hashService)
        {
            _records = new List<FileRecord>();
            _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
        }

        public void LoadRecords(List<FileRecord> records)
        {
            if (records == null) throw new ArgumentNullException(nameof(records));
            _records = records;
        }

        public FileRecord RegisterFile(string filePath, string algorithm = "SHA256")
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath)) throw new FileNotFoundException($"File not found: {filePath}");

            try
            {
                byte[] content = File.ReadAllBytes(filePath);
                var info = new FileInfo(filePath);
                string hash = ComputeFileHash(content, algorithm);

                var record = new FileRecord(filePath, hash, algorithm, info.Length);
                _records.Add(record);
                return record;
            }
            catch (IOException ex)
            {
                throw new IOException($"Failed to read file '{filePath}'", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException($"Access denied to file '{filePath}'", ex);
            }
        }

        public bool VerifyFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            var record = _records.Find(r => r.FilePath == filePath);
            if (record == null)
                throw new InvalidOperationException($"File '{filePath}' is not registered.");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            try
            {
                byte[] content = File.ReadAllBytes(filePath);
                string currentHash = ComputeFileHash(content, record.Algorithm);
                record.LastCheckedAt = DateTime.Now;

                bool isIntact = string.Equals(record.OriginalHash, currentHash, StringComparison.OrdinalIgnoreCase);
                if (!isIntact)
                    throw new DataMisalignedException($"File '{filePath}' has been tampered. Original: {record.OriginalHash}, Current: {currentHash}");

                return true;
            }
            catch (IOException ex)
            {
                throw new IOException($"Error reading file '{filePath}'", ex);
            }
        }

        private string ComputeFileHash(byte[] content, string algorithm)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (string.IsNullOrEmpty(algorithm)) throw new ArgumentException("Algorithm cannot be empty", nameof(algorithm));

            return algorithm.ToUpperInvariant() switch
            {
                "SHA256" => _hashService.ComputeSha256(content),
                "MD5" => _hashService.ComputeMd5(content),
                _ => throw new NotSupportedException($"Algorithm '{algorithm}' not supported for file hashing")
            };
        }

        public List<FileRecord> GetTamperedFiles()
        {
            var result = new List<FileRecord>();
            for (int i = 0; i < _records.Count; i++)
            {
                try
                {
                    if (!VerifyFile(_records[i].FilePath))
                        result.Add(_records[i]);
                }
                catch (Exception)
                {
                    // Если файл недоступен или удалён, считаем его нарушенным
                    result.Add(_records[i]);
                }
            }
            return result;
        }

        public List<FileRecord> GetAll() => _records;
    }
}