using System;
using System.IO;

namespace Lingua.Core
{
    public static class LoaderBase
    {
        private const string BaseDir = "Languages";
        private const string LanguageDir = "EnglishSwedish";

        public static string[] ReadFile(string relativePath)
        { 
            var projectDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var solutionDir = projectDir.Parent?.Parent?.Parent;
            var filePath = Path.Combine(solutionDir?.FullName ?? "", BaseDir, LanguageDir, relativePath);
            return File.ReadAllLines(filePath);
        }
    }
}