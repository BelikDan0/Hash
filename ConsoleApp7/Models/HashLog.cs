using System;

namespace HashSystem.Models
{
    public class HashLog
    {
        public string Id { get; set; }
        public string Operation { get; set; }
        public string Algorithm { get; set; }
        public string InputPreview { get; set; }
        public string? ResultHash { get; set; }   // допускает null
        public bool Success { get; set; }
        public DateTime Timestamp { get; set; }
        public string? ErrorMessage { get; set; } // допускает null

        public HashLog(string id, string operation, string algorithm, string inputPreview)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Operation = operation ?? throw new ArgumentNullException(nameof(operation));
            Algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
            InputPreview = inputPreview ?? "";
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {Operation} ({Algorithm}) — {(Success ? "OK" : "FAIL")}: {ErrorMessage ?? ResultHash ?? "no data"}";
        }
    }
}