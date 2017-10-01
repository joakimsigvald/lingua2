using System.Collections.Generic;
using System.Linq;
using Lingua.Core;

namespace Lingua.Grammar
{
    public static class Loader
    {
        public static IDictionary<string, int> LoadScoredPatterns()
        {
            var lines = LoaderBase.ReadFile("Patterns.txt")
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("//"));
            return lines.Select(line => line.Split(':'))
                .ToDictionary(pair => pair[0], pair => int.Parse(pair[1]));
        }

        public static Dictionary<string, string> LoadRearrangements()
        {
            var lines = LoaderBase.ReadFile("Rearrangements.txt")
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("//"));
            return lines.Select(line => line.Split(':'))
                .ToDictionary(pair => pair[0], pair => pair[1]);
        }
    }
}