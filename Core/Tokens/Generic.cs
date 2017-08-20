using Lingua.Core.WordClasses;

namespace Lingua.Core.Tokens
{
    public class Generic : Token
    {
        public Generic(string stem) : this(new Unclassified {Value = stem})
        {
        }

        public Generic(Word stem) => Stem = stem;
        public Word Stem { get; }
    }
}
