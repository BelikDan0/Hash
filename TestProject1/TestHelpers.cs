using System;
using System.IO;



    public static class TestHelpers
    {
        private static readonly Random _random = new Random();

        public static string CreateTempFile(string content, string extension = ".txt")
        {
            string fileName = $"test_{_random.Next():x8}{extension}";
            File.WriteAllText(fileName, content);
            return fileName;
        }

        public static void DeleteTempFile(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
