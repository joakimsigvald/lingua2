using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;

namespace Lingua.Grammar
{
    public class Scorer
    {
        private static readonly IDictionary<string, int> ScoredPatterns 
            = Loader.LoadScoredPatterns();

        private static readonly IList<Scorer> Scorers = ScoredPatterns.Select(sp => new Scorer
        {
            Pattern = Encoder.Encode(Encoder.Deserialize(sp.Key)).ToArray(),
            Score = sp.Value
        }).ToList();

        public static Evaluation Evaluate(IList<Token> tokens)
        {
            var code = Encode(tokens);
            return Evaluate(tokens, code);
        }

        private static ushort[] Encode(IEnumerable<Token> tokens)
        {
            var deconjunctedTokens = Trim(tokens);
            return Encoder.Encode(deconjunctedTokens.Prepend(Start.Singleton)).ToArray();
        }

        private static Evaluation Evaluate(IList<Token> tokens, ushort[] code)
        {
            var scorings = Scorers.Select(s
                    => new Scoring(s.Pattern, s.CountMatches(code), s.Score))
                .Where(m => m.Count > 0)
                .ToArray();
            var score = scorings.Sum(s => s.TotalScore);
            return new Evaluation(tokens, score, scorings);
        }

        private static IEnumerable<Token> Trim(IEnumerable<Token> tokens)
            => MergeConjunctions(tokens.Where(token => !(token is Divider)).ToArray());

        private static IEnumerable<Token> MergeConjunctions(ICollection<Token> tokens)
        {
            if (tokens.Count < 3)
                return tokens;
            (var a, var b, var c) = tokens.Take(3);
            var isHomogeneous = IsHomogeneousConjunction(a, b, c);
            return isHomogeneous
                ? MergeConjunctions(tokens.Skip(3).ToArray()).Prepend(a)
                : MergeConjunctions(tokens.Skip(1).ToArray()).Prepend(a);
        }

        private static bool IsHomogeneousConjunction(Token a, Token c, Token b)
            => c is Conjunction && IsSameWordForm(a as Word, b as Word);

        private static bool IsSameWordForm(Word a, Word b)
            => a != null && b != null && a.GetType() == b.GetType() && a.Modifiers == b.Modifiers;

        private ushort[] Pattern { get; set; }
        private int Score { get; set; }

        private int CountMatches(ushort[] sequence)
            => sequence.Length < Pattern.Length
                ? 0
                : Enumerable.Range(0, 1 + sequence.Length - Pattern.Length)
                    .Count(n => Encoder.Matches(sequence.Skip(n).Take(Pattern.Length).ToArray(), Pattern));
    }
}