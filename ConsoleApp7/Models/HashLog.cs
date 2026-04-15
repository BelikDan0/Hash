using System;

namespace HashSystem.Models
{
    /// <summary>
    /// Запись в журнале операций хеширования.
    /// </summary>
    public class HashLog
    {
        /// <summary>Уникальный идентификатор операции.</summary>
        public string Id { get; set; }

        /// <summary>Название операции (например, "ComputeSha256").</summary>
        public string Operation { get; set; }

        /// <summary>Используемый алгоритм.</summary>
        public string Algorithm { get; set; }

        /// <summary>Превью входных данных (первые символы).</summary>
        public string InputPreview { get; set; }

        /// <summary>Результат хеширования (может быть null при ошибке).</summary>
        public string? ResultHash { get; set; }

        /// <summary>Флаг успешности операции.</summary>
        public bool Success { get; set; }

        /// <summary>Время выполнения операции.</summary>
        public DateTime Timestamp { get; set; }

        /// <summary>Сообщение об ошибке (если операция не удалась).</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Создаёт новую запись журнала.
        /// </summary>
        /// <param name="id">Идентификатор.</param>
        /// <param name="operation">Операция.</param>
        /// <param name="algorithm">Алгоритм.</param>
        /// <param name="inputPreview">Превью входных данных.</param>
        public HashLog(string id, string operation, string algorithm, string inputPreview)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Operation = operation ?? throw new ArgumentNullException(nameof(operation));
            Algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
            InputPreview = inputPreview ?? "";
            Timestamp = DateTime.Now;
        }

        /// <summary>Форматированное строковое представление записи.</summary>
        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {Operation} ({Algorithm}) — {(Success ? "OK" : "FAIL")}: {ErrorMessage ?? ResultHash ?? "no data"}";
        }
    }
}