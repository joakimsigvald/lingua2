using Lingua.Core.Extensions;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lingua.Core
{
    public class TokenGenerator
    {
        private readonly ITokenizer _tokenizer;
        private static readonly IDictionary<string, string> Expanders = LoadExpanders();

        public TokenGenerator(ITokenizer tokenizer) => _tokenizer = tokenizer;

        public Token[] GetTokens(string original) 
            => Expand(Tokenize(original)).ToArray();

        private IEnumerable<Token> Expand(IEnumerable<Token> tokens)
            => tokens.SelectMany(Expand);

        private IEnumerable<Token> Expand(Token token)
            => Tokenize(Expand(token as Unclassified)) ?? new[] { token };

        private IEnumerable<Token>? Tokenize(string? text)
            => text is null ? null : _tokenizer.Tokenize(text);

        private static string? Expand(Unclassified? word)
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
            var lines = ReadExpanerLines();
            return lines.Select(ParseExpander)
                .ToDictionary(expander => expander.from, expander => expander.to);
        }

        private static (string from, string to) ParseExpander(string line)
        {
            var parts = Regex.Split(line, " => ");
            return (parts[0].Trim(), parts[1].Trim());
        }

        private static IEnumerable<string> ReadExpanerLines()
            => LoaderBase.ReadFile("Expanders.txt");
    }
}