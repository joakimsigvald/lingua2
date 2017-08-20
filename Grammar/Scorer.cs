using System.Linq;

namespace Lingua.Grammar
{
    public class Scorer
    {
        public ushort[] Pattern { get; set; }
        public int Score { get; set; }

        public int CountMatches(ushort[] sequence)
            => sequence.Length < Pattern.Length
                ? 0
                : Enumerable.Range(0, 1 + sequence.Length - Pattern.Length)
                    .Count(n => Matches(sequence.Skip(n).Take(Pattern.Length).ToArray(), Pattern));

        private static bool Matches(ushort[] segment, ushort[] pattern)
            => segment.Select((d, i) => d == pattern[i]).All(b => b);
    }
}