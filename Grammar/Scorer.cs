using System.Linq;
using Lingua.Core;

namespace Lingua.Grammar
{
    internal class Scorer
    {
        internal ushort[] Pattern { get; set; }
        internal int Score { get; set; }

        internal int CountMatches(ushort[] sequence)
            => sequence.Length < Pattern.Length
                ? 0
                : Enumerable.Range(0, 1 + sequence.Length - Pattern.Length)
                    .Count(n => Encoder.Matches(sequence.Skip(n).Take(Pattern.Length).ToArray(), Pattern));
    }
}