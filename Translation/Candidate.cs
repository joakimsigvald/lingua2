using System.Collections;
using System.Collections.Generic;

namespace Lingua.Core
{
    public class TranslationCandidate : ITranslationCandidate
    {
        private readonly ITranslation[] _translations;

        public TranslationCandidate(params ITranslation[] translations) => _translations = translations;

        public ITranslation this[int index] => _translations[index];

        public int Length => _translations.Length;

        public IEnumerator<ITranslation> GetEnumerator()
            => ((IEnumerable<ITranslation>)_translations).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _translations.GetEnumerator();
    }

    public class GrammatonCandidate : IGrammatonCandidate
    {
        private readonly IGrammaton[] _grammaton;

        public GrammatonCandidate(params IGrammaton[] grammaton) => _grammaton = grammaton;

        public IGrammaton this[int index] => _grammaton[index];

        public int Length => _grammaton.Length;

        public IEnumerator<IGrammaton> GetEnumerator()
            => ((IEnumerable<IGrammaton>)_grammaton).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _grammaton.GetEnumerator();
    }
}