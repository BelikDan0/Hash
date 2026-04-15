using System;
using System.IO;

/// <summary>
/// Вспомогательные методы для создания и удаления временных файлов в тестах.
/// </summary>
public static class TestHelpers
{
    private static readonly Random _random = new Random();

    /// <summary>
    /// Создаёт временный файл с заданным содержимым и возвращает его полный путь.
    /// </summary>
    /// <param name="content">Содержимое файла.</param>
    /// <param name="extension">Расширение файла (по умолчанию ".txt").</param>
    /// <returns>Путь к созданному файлу.</returns>
    public static string CreateTempFile(string content, string extension = ".txt")
    {
        string fileName = $"test_{_random.Next():x8}{extension}";
        File.WriteAllText(fileName, content);
        return fileName;
    }

    /// <summary>
    /// Удаляет временный файл, если он существует.
    /// </summary>
    /// <param name="path">Путь к удаляемому файлу.</param>
    public static void DeleteTempFile(string path)
    {
        if (File.Exists(path)) File.Delete(path);
    }
}