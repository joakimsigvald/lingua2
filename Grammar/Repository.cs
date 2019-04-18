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

        public static void StoreScoredPatterns(string[] patterns)
            => WriteLines(GetUniqueName("Patterns.txt"), patterns);

        public static void StoreRearrangements(IEnumerable<Arranger> arrangers)
            => WriteLines(GetUniqueName("Rearrangements.txt")
                , GetRearrangementLines(arrangers.Select(arr => arr.Arrangement)));

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

        public static void StoreTestCases(string tag, string[] testCases)
            => WriteLines(GetUniqueName($"TestCases-{tag}.txt"), testCases);

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