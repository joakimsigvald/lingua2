using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Lingua.Core
{
    public static class LoaderBase
    {
        private const string BaseDir = "Languages";
        private const string LanguageDir = "EnglishSwedish";

        public static string[] ReadFile(string relativePath)
        {
            var directoy = GetDirectoryPath();
            var filePath = GetPath(directoy, relativePath);
            return File.ReadAllLines(filePath, Encoding.UTF8)
                .Select(line => line.Trim())
                .ToArray();
        }

        public static void WriteToFile(string relativePath, string[] lines)
        {
            var directoy = GetDirectoryPath();
            var filePath = GetPath(directoy, relativePath);
            File.WriteAllLines(filePath, lines, Encoding.UTF8);
        }

        public static string GetUniqueName(string basename)
        {
            var directory = GetDirectoryPath();
            var parts = basename.Split('.');
            return GetUniqueName(directory, parts[0], parts[1]);
        }

        private static string GetUniqueName(string directory, string name, string extension)
        {
            var n = 1;
            string path;
            string filename;
            do path = GetPath(directory, filename = $"{name}{n++}.{extension}");
            while (File.Exists(path));
            return filename;
        }

        private static string GetPath(string directory, string relativePath) 
            => Path.Combine(directory, relativePath);

        private static string GetDirectoryPath()
        {
            var projectDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var solutionDir = projectDir.Parent?.Parent?.Parent;
            return Path.Combine(solutionDir?.FullName ?? "", BaseDir, LanguageDir);
        }
    }
}