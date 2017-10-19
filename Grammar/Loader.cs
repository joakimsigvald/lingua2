using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core;

    public static class Loader
    {
        public static IDictionary<string, sbyte> LoadScoredPatterns()
            => ReadLines("Patterns.txt").ToDictionary(sbyte.Parse);

        public static Dictionary<string, string> LoadRearrangements()
            => ReadLines("Rearrangements.txt").ToDictionary(v => v);

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