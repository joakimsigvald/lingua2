using Lingua.Core;
using Lingua.Core.Extensions;
using Lingua.Core.WordClasses;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lingua.Tokenization
{
    public class WordExpander
    {
        private static readonly IDictionary<string, string> Expanders = LoadExpanders();

        public static string? TryExpand(Unclassified? word)
            => word is null
                ? null
                : TryExpand(word.Value, out string exactExpanded)
                    ? exactExpanded
                    : TryExpand(word.Value.ToLower(), out string lowerExpanded)
                        ? lowerExpanded.Capitalize()
                        : null;

        private static bool TryExpand(string word, out string expanded)
            => Expanders.TryGetValue(word, out expanded);

        private static IDictionary<string, string> LoadExpanders()
        {
            var lines = ReadExpanderLines();
            return lines.Select(ParseExpander)
                .ToDictionary(expander => expander.from, expander => expander.to);
        }

        private static (string from, string to) ParseExpander(string line)
        {
            var parts = Regex.Split(line, " => ");
            return (parts[0].Trim(), parts[1].Trim());
        }

        private static IEnumerable<string> ReadExpanderLines()
            => LoaderBase.ReadFile("Expanders.txt");
    }
}