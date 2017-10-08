using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core;

namespace Lingua.Grammar
{
    public static class Loader
    {
        public static IDictionary<string, int> LoadScoredPatterns()
            => ReadLines("Patterns.txt").ToDictionary(int.Parse);

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