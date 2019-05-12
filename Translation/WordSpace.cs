using Lingua.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Translation
{
    public class WordSpace : IWordSpace
    {
        private readonly ITranslationCandidate[] _translationCandidates;

        public IGrammatonCandidate[] GrammatonCandidates { get; }

        public WordSpace(ITranslation[] translations)
        {
            _translationCandidates = translations.Select(CreateTranslationCandidate).ToArray();
            GrammatonCandidates = CreateMultiGrammatonCandidates(
                _translationCandidates.Where(tc => tc.Length > 1))
                .Concat(CreateSingleGrammatonCandidates(
                    _translationCandidates.Where(tc => tc.Length == 1)))
                .ToArray();
        }

        private IEnumerable<IGrammatonCandidate> CreateMultiGrammatonCandidates(
            IEnumerable<ITranslationCandidate> multiTranslationCandidates)
            => multiTranslationCandidates.Select(CreateGrammatonCandidate);

        private IEnumerable<IGrammatonCandidate> CreateSingleGrammatonCandidates(
            IEnumerable<ITranslationCandidate> singleTranslationCandidates)
            => CreateSingleGrammatons(singleTranslationCandidates.Select(tc => tc[0]))
                .Select(g => new GrammatonCandidate(g))
                .ToArray();

        private IEnumerable<IGrammaton> CreateSingleGrammatons(IEnumerable<ITranslation> translations)
            => translations.GroupBy(t => (t.Code, t.WordCount)).Select(g => new Grammaton(g.ToArray()));

        private IGrammatonCandidate CreateGrammatonCandidate(ITranslationCandidate tc)
            => new GrammatonCandidate(tc.Select(t => new Grammaton(t)).ToArray());

        public WordSpace(IEnumerable<ITranslationCandidate> candidates)
            => _translationCandidates = candidates.ToArray();

        private ITranslationCandidate CreateTranslationCandidate(ITranslation translation)
            => new TranslationCandidate(translation);

        public IEnumerator<ITranslationCandidate> GetEnumerator()
            => ((IEnumerable<ITranslationCandidate>)_translationCandidates).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _translationCandidates.GetEnumerator();
    }
}