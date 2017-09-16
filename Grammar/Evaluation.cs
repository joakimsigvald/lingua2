using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Lingua.Core.Tokens;

namespace Lingua.Grammar
{
    public class Evaluation : IEvaluation
    {
        public Evaluation(IList<Token> tokens, int score, IEnumerable<Scoring> scorings)
        {
            Fragment = string.Join("", tokens.Select(token => token.Value));
            Symbols = Encoder.Serialize(tokens);
            Score = score;
            Patterns = scorings.Select(s => s.Pattern).ToArray();
        }

        public string Fragment { get; }
        public string Symbols { get; }
        private string[] Patterns { get; }
        public int Score{ get; }

        public override string ToString() => $"{Fragment}|{Symbols}|{string.Join(",", Patterns)}:{Score}";
    }
}