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
                : from.Value.Split(' ')
                    .Skip(1)
                    .Select(part => new Unclassified {Value = part})
                    .Cast<Word>()
                    .ToArray();

        public Translation()
        {
        }

        private Translation(Token from, string to, Word[] continuation)
        {
            From = from;
            To = to;
            Continuation = continuation;
        }

        public Token From { get; set; }
        public string To { get; set; }
        public Word[] Continuation { get; set; } = new Word[0];
        public Translation[] Variations { get; set; } = new Translation[0];
        public byte WordCount => (byte)(Continuation.Length + 1);
        public bool IsTranslatedWord => To != null && !Continuation.Any();

        public Translation Capitalize() 
            => new Translation(From.Capitalize(), To.Capitalize(), Continuation)
            {
                IsCapitalized = true
            };

        public bool IsInvisibleCapitalized => IsCapitalized && IsInvisible;

        public bool IsCapitalized { get; private set; }
        private bool IsInvisible => string.IsNullOrEmpty(Output);
        public string Output => To ?? From.Value;
        public bool IsIncompleteCompound { get; set; }

        public override string ToString() => $"{From}->{To}{(Continuation.Any() ? "..." : "")}";

        public bool Matches(IReadOnlyList<Token> tokens, int nextIndex)
            => IsTranslatedWord || MatchesContinuation(tokens, nextIndex);

        private bool MatchesContinuation(IReadOnlyList<Token> tokens, int nextIndex)
            => tokens.Count >= nextIndex + Continuation.Length
                   && Continuation.All(word => Matches(word, tokens, ref nextIndex));

        private static bool Matches(Word word, IReadOnlyList<Token> tokens, ref int nextIndex)
            => tokens[nextIndex++] is Word next && next.Value == word.Value;
    }
}