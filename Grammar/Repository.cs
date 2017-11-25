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

        public static void StoreScoredPatterns(IDictionary<string, sbyte> patterns)
            => WriteLines(GetUniqueName("Patterns.txt")
                , GetPatternLines(patterns));

        private static string[] GetPatternLines(IDictionary<string, sbyte> patterns)
        {
            return patterns
                .OrderByDescending(sp => sp.Value)
                .ThenBy(sp => sp.Key)
                .Select(ToLine).ToArray();
        }

        private static void WriteLines(string filename, string[] lines)
            => LoaderBase.WriteToFile(filename, lines);

        private static string GetUniqueName(string filename)
            => LoaderBase.GetUniqueName(filename);

        private static string ToLine(KeyValuePair<string, sbyte> scoredPattern)
            => $"{scoredPattern.Key}:{scoredPattern.Value}";

        public static Dictionary<string, byte[]> LoadRearrangements()
            => ReadLines("Rearrangements.txt")
            .ToDictionary(v => v.Select(c => (byte)(c - 49)).ToArray());

        private static Dictionary<string, T> ToDictionary<T>(
            this IEnumerable<string> lines, Func<string, T> convert)
            => lines.Select(Split).ToDictionary(convert);

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