using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Lingua.Core.Tokens;

namespace Lingua.Grammar
{
    public class Scorer
    {
        /// <Types>
        /// T: ArTicle
        /// R: Pronoun
        /// N: Noun
        /// A: Adjective
        /// V: Verb
        /// X: Auxilery
        /// Q: quantifier (number)
        /// .: Terminator
        /// ,: Separator
        /// </Types>

        /// <Modifiers>
        /// d: definite
        /// n: plural
        /// g: genitive
        /// 1: 1st person
        /// 2: 2nd person
        /// 3: 3rd person
        /// </Modifiers>
        private static readonly IDictionary<string, int> ScoredPatterns = new Dictionary<string, int>
        {
            { "TdnqAdnNdn", 2},
            { "TdnqQAdnNdn", 2},
            { "XfVf", 2},
            { "XfrVdr", 2},
            { "TdNd", 1},
            { "TdNdn", 1},
            { "TdNdg", 1},
            { "TdNdng", 1},
            { "QnNn", 1},
            { "TdqAdnNd", 1},
            { "TdqAcdNd", 1},
            { "TdqAdsNd", 1},
            { "TdnqAdsNdn", 1},
            { "AdnNn", 1},
            { "R*V1", 1},
            { "R3V3", 1},
            { "R3nV3n", 1},
            { "VpAa", 1},
            { "R1X1Vr", 1},
            { "R1X1XpVdr", 1},
            { "R2X2XpVdr", 1},
            { "R3X3XpVdr", 1},
            { "R3nX3nXpVdr", 1},
            { "R*XpXpVdr", 1},
            { "R*X*Vd", 1},
            { "R2X2Vr", 1},
            { "R3X3Vr", 1},
            { "R3nXn3Vr", 1},
            { "R*XpVr", 1},
            { "^Vi", 1},
            { "R3tX3R*N", 1},
            { "IV", 1},
            { "NN", -1},
            { "NNd", -1},
            { "NNdn", -1},
            { "NNn", -1},
            { "NdN", -1},
            { "NdNd", -1},
        };

        private static readonly IList<Scorer> Scorers = ScoredPatterns.Select(sp => new Scorer
        {
            Pattern = Encoder.Encode(Encoder.Deserialize(sp.Key)).ToArray(),
            Score = sp.Value
        }).ToList();

        public static Evaluation Evaluate(IList<Token> tokens)
        {
            var code = Encoder.Encode(tokens.Prepend(Start.Singleton)).ToArray();
            var scorings = Scorers.Select(s 
                => new Scoring(s.Pattern, s.CountMatches(code), s.Score))
                .Where(m => m.Count > 0)
                .ToArray();
            var score = scorings.Sum(s => s.TotalScore);
            return new Evaluation(tokens, score, scorings);
        }

        private int[] Pattern { get; set; }
        private int Score { get; set; }

        private int CountMatches(int[] sequence)
            => sequence.Length < Pattern.Length
                ? 0
                : Enumerable.Range(0, 1 + sequence.Length - Pattern.Length)
                    .Count(n => Encoder.Matches(sequence.Skip(n).Take(Pattern.Length).ToArray(), Pattern));
    }
}