using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Extensions;
using Lingua.Core.WordClasses;
using Lingua.Tokenization.Symbols;

namespace Lingua.Tokenization
{
    using Core;
    using Core.Tokens;

    public class Tokenizer : ITokenizer
    {
        private static readonly ISymbolizer Symbolizer = new Symbolizer();

        public IEnumerable<Token> Tokenize(string text)
            => string.IsNullOrWhiteSpace(text)
                ? new Token[0]
                : CombineGenerics(Trim(Tokenize(Symbolizer.Symbolize(text).ToArray())));

        private static IEnumerable<Token> Tokenize(IReadOnlyList<Symbol> symbols)
        {
            var prev = symbols[0];
            var current = symbols[1];
            var original = $"{prev.Character}";
            var token = Symbolizer.GetNextToken(prev, NoSymbol.Singleton, current);
            foreach (var next in symbols.Skip(2))
            {
                if (Symbolizer.TryGetNextToken(current, prev, next, out Token nextToken))
                {
                    yield return GetEnriched(token, original, current);
                    token = nextToken;
                    original = "";
                }
                original += current.Character;
                prev = current;
                current = next;
            }
            yield return GetEnriched(token, original, current);
        }

        private static IEnumerable<Token> Trim(IEnumerable<Token> tokens)
            => tokens.Where(token => !(token is Divider));

        private static IEnumerable<Token> CombineGenerics(IEnumerable<Token> tokens)
        {
            var startedGeneric = false;
            var yieldedGeneric = false;
            foreach (var token in tokens)
            {
                if (token is StartGeneric)
                {
                    if (startedGeneric || yieldedGeneric)
                        throw new Exception("Invalid generic");
                    startedGeneric = true;
                }
                else if (startedGeneric)
                {
                    var word = token as Word;
                    if (word == null)
                        throw new Exception("Invalid generic");
                    startedGeneric = false;
                    yieldedGeneric = true;
                    yield return new Generic(word);
                }
                else if (yieldedGeneric)
                {
                    if (!(token is EndGeneric))
                        throw new Exception("Invalid generic");
                    yieldedGeneric = false;
                }
                else yield return token;
            }
        }

        private static Token GetEnriched(Token token, string original, Symbol next)
        {
            token.Value = original;
            if ((token is StartGeneric || token is EndGeneric) && token.Value.Length != 2)
                throw new NotImplementedException("Unknown token: " + token.Value);
            if (!(token is Unclassified word)) return token;
            if (word.Value.IsCapitalized())
                token = word = new Name {Value = word.Value};
            word.PossibleAbbreviation = next is Dot;
            return token;
        }
    }
}