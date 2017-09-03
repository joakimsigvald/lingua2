using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Tokens;

namespace Lingua.Grammar
{
    public class Scorer
    {
        /// <Types>
        /// A: Adjective
        /// V: Verb
        /// N: Noun
        /// T: ArTicle
        /// Q: quantifier (number)
        /// .: Terminator
        /// ,: Separator
        /// </Types>

        /// <Modifiers>
        /// d: definite
        /// n: plural
        /// p: possessive
        /// </Modifiers>
        private static readonly IDictionary<string, int> ScoredPatterns = new Dictionary<string, int>
        {
            { "TdNd", 1},
            { "TdNdn", 1},
            { "TdNdp", 1},
            { "TdNdnp", 1},
            { "QnNn", 1},
            { "TdqAdNd", 1},
            { "NN", -1},
            { "NNd", -1},
            { "NNdn", -1},
            { "NNn", -1},
            { "NdN", -1},
            { "NdNd", -1},
        };

        private static readonly IList<Scorer> Scorers = ScoredPatterns.Select(sp => new Scorer
        {
            Pattern = Encoder.Code(Encoder.Deserialize(sp.Key)).ToArray(),
            Score = sp.Value
        }).ToList();

        public static int Compute(IEnumerable<Token> tokens)
        {
            var str = Encoder.Serialize(tokens); // for debugging
            var code = Encoder.Code(tokens).ToArray();
            var score = Scorers.Sum(s => s.CountMatches(code) * s.Score);
            return score;
        }

        private int[] Pattern { get; set; }
        private int Score { get; set; }

        private int CountMatches(int[] sequence)
            => sequence.Length < Pattern.Length
                ? 0
                : Enumerable.Range(0, 1 + sequence.Length - Pattern.Length)
                    .Count(n => Matches(sequence.Skip(n).Take(Pattern.Length).ToArray(), Pattern));

        private static bool Matches(int[] segment, int[] pattern)
            => segment.Select((d, i) => d == pattern[i]).All(b => b);
    }
}