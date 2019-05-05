using Lingua.Core;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Tokenization
{
    public class TokenGenerator : ITokenGenerator
    {
        private readonly ITokenizer _tokenizer;

        public TokenGenerator(ITokenizer tokenizer) => _tokenizer = tokenizer;

        public Token[] GetTokens(string? original)
            => Expand(_tokenizer.Tokenize(original)).ToArray();

        private IEnumerable<Token> Expand(IEnumerable<Token> tokens)
            => tokens.SelectMany(Expand);

        private IEnumerable<Token> Expand(Token token)
        {
            var expandedUnclassified = WordExpander.TryExpand(token as Unclassified);
            return expandedUnclassified is null 
                ? new[] { token } 
                : _tokenizer.Tokenize(expandedUnclassified);
        }
    }
}