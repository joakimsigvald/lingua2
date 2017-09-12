﻿using System.Collections.Generic;
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
            { "R1V1", 1},
            { "R1nV1", 1},
            { "R2V1", 1},
            { "R2nV1", 1},
            { "R3V3", 1},
            { "R3nV3", 1},
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