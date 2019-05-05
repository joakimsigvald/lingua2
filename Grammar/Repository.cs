using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    public static class Repository
    {
        public static IDictionary<string, sbyte> LoadScoredPatterns()
            => ReadLines("Patterns.txt").ToDictionary(sbyte.Parse);

        public static string[] Load(string filename)
            => ReadLines(filename).ToArray();

        public static void StoreScoredPatterns(string[] patterns, bool overwrite = false)
            => StoreText("Patterns", patterns, overwrite);

        public static void StoreRearrangements(IEnumerable<Arrangement> arrangers, bool overwrite = false)
            => StoreText("Rearrangements", GetRearrangementLines(arrangers), overwrite);

        private static string[] GetRearrangementLines(IEnumerable<Arrangement> arrangements)
            => arrangements
                .OrderBy(arr => arr.Length)
                .ThenBy(arr => arr.Pattern)
                .Select(arr => arr.Serialize()).ToArray();

        private static void WriteLines(string filename, string[] lines)
            => LoaderBase.WriteToFile(filename, lines);

        private static string GetUniqueName(string filename)
            => LoaderBase.GetUniqueName(filename);

        public static IEnumerable<Arrangement> LoadArrangements()
            => ReadLines("Rearrangements.txt").Select(Arrangement.Deserialize);

        private static Dictionary<string, T> ToDictionary<T>(
            this IEnumerable<string> lines, Func<string, T> convert)
            => lines.Select(Split).ToDictionary(convert);

        public static void StoreText(string name, string[] lines, bool overwrite = false)
            => WriteLines(GetFilename(name, overwrite), lines);

        private static string GetFilename(string name, bool overwrite = false)
            => overwrite 
            ? $"{name}.txt" 
            : GetUniqueName(GetFilename(name));

        private static Dictionary<string, T> ToDictionary<T>(
            this IEnumerable<string[]> pairs, Func<string, T> convert)
            => pairs.ToDictionary(pair => pair[0], pair => convert(pair[1]));

        private static string[] Split(string line)
            => line.Split(':');

        private static IEnumerable<string> ReadLines(string filename)
            => LoaderBase.ReadFile(filename).Where(HasData);

        private static bool HasData(string line)
            => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("//");
    }
}