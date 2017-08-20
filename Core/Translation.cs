using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;

namespace Lingua.Core
{
    public class Translation
    {
        public static Translation Create(Token from, string to = null)
            => new Translation(from, to, CreateContinuation(from as Word));

        private static Word[] CreateContinuation(Word from)
            => string.IsNullOrWhiteSpace(from?.Value)
                ? new Word[0]
                : from.Value.Split(new[] {' '}, 2)
                    .Skip(1)
                    .Select(part => new Unclassified {Value = part})
                    .Cast<Word>()
                    .ToArray();

        private Translation(Token from, string to, Word[] continuation)
        {
            From = from;
            To = to;
            Continuation = continuation;
        }

        public Token From { get; }
        public string To { get; }
        private Word[] Continuation { get; }
        public Translation[] Variations { get; set; } = new Translation[0];
        public int TokenCount => WordCount * 2 - 1;
        public int WordCount => Continuation.Length + 1;
        public bool IsTranslatedWord => To != null && !Continuation.Any();

        public Translation Capitalize() 
            => new Translation(From.Capitalize(), To.Capitalize(), Continuation)
            {
                IsCapitalized = true
            };

        public bool IsInvisibleCapitalized => IsCapitalized && IsInvisible;

        private bool IsCapitalized { get; set; }
        private bool IsInvisible => string.IsNullOrEmpty(Output);
        public string Output => To ?? From.Value;

        public override string ToString() => Output + (Continuation.Any() ? "..." : "");

        public bool Matches(IReadOnlyList<Token> tokens, int nextIndex)
            => IsTranslatedWord || MatchesContinuation(tokens, nextIndex);

        private bool MatchesContinuation(IReadOnlyList<Token> tokens, int nextIndex)
            => tokens.Count >= nextIndex + Continuation.Length * 2
               && Continuation.All(word => Matches(word, tokens, ref nextIndex));

        private static bool Matches(Word word, IReadOnlyList<Token> tokens, ref int nextIndex)
            => tokens[nextIndex++] is Divider && (tokens[nextIndex++] as Word)?.Value == word.Value;
    }
}