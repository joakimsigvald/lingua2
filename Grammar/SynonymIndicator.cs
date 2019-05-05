using System;
using System.Linq;

namespace Lingua.Grammar
{
    public class SynonymIndicator
    {
        public SynonymIndicator(string word, string? previous, string? next)
        {
            Word = word;
            PreviousWord = previous;
            NextWord = next;
        }

        public string Word { get; }
        public string? PreviousWord { get; }
        public string? NextWord { get; }

        public bool Matches(string previous, string[] next)
            => (PreviousWord is null || PreviousWord == previous)
                && (NextWord is null || next.Contains(NextWord));

        internal static SynonymIndicator Create(string expression)
        {
            var parts1 = expression.Split('<');
            if (parts1.Length == 1)
            {
                parts1 = expression.Split('>');
                return new SynonymIndicator(parts1[0], null, parts1[1]);
            }
            var parts2 = parts1.Last().Split('>');
            return new SynonymIndicator(parts1[0], parts2[0], parts2.Skip(1).SingleOrDefault());
        }
    }
}