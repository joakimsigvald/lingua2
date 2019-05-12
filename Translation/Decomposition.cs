using Lingua.Translation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core
{
    public class Decomposition : IDecomposition
    {
        private readonly IWordSpace[] _wordSpaces;

        public Decomposition(IEnumerable<ITranslation[]> possibilities) 
            => _wordSpaces = possibilities.Select(CreateWordSpace).ToArray();

        public Decomposition(IEnumerable<IWordSpace> wordSpaces) 
            => _wordSpaces = wordSpaces.ToArray();

        private WordSpace CreateWordSpace(ITranslation[] translations)
            => new WordSpace(translations);

        public IEnumerator<IWordSpace> GetEnumerator()
            => ((IEnumerable<IWordSpace>)_wordSpaces).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _wordSpaces.GetEnumerator();

        public IWordSpace this[int index] => _wordSpaces[index];

        public bool IsEmpty => Length == 0;
        public int Length => _wordSpaces.Length;
    }
}