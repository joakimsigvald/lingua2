using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Lingua.Core.Tokens;

namespace Lingua.Grammar
{
    public class Evaluation : IEvaluation
    {
        public Evaluation(IList<Token> tokens, int score)
        {
            Fragment = string.Join("", tokens.Select(token => token.Value));
            Symbols = Encoder.Serialize(tokens);
            Score = score;
        }

        public string Fragment { get; }
        public string Symbols { get; }
        public int Score{ get; }

        public override string ToString() => $"{Fragment}|{Symbols}|{Score}";
    }
}