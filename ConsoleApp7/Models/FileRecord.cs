using System;

namespace HashSystem.Models
{
    public class FileRecord
    {
        public string FilePath { get; set; }
        public string OriginalHash { get; set; }
        public string Algorithm { get; set; }
        public long FileSize { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? LastCheckedAt { get; set; }

        public FileRecord(string filePath, string originalHash,
                          string algorithm, long fileSize)
        {
            FilePath     = filePath;
            OriginalHash = originalHash;
            Algorithm    = algorithm;
            FileSize     = fileSize;
            RegisteredAt = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{FilePath}] {Algorithm}: " +
                   $"{OriginalHash[..8]}... размер: {FileSize} байт";
        }
    }
}