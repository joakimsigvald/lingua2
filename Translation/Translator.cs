using System.Collections.Generic;
using System.Linq;
using Lingua.Core.WordClasses;

namespace Lingua.Core
{
    using Lingua.Translation;
    using Tokens;

    public class Translator : ITranslator
    {
        private readonly IThesaurus _thesaurus;
        private readonly IGrammar _grammar;
        private readonly IArranger _arranger;
        private readonly ISynonymResolver _synonymResolver;
        private readonly ICapitalizer _capitalizer;
        private readonly ITokenGenerator _tokenGenerator;

        public Translator(
            ITokenGenerator tokenGenerator, 
            IThesaurus thesaurus, 
            IGrammar grammar,
            IArranger arranger,
            ISynonymResolver synonymResolver,
            ICapitalizer capitalizer)
        {
            _thesaurus = thesaurus;
            _grammar = grammar;
            _arranger = arranger;
            _synonymResolver = synonymResolver;
            _tokenGenerator = tokenGenerator;
            _capitalizer = capitalizer;
        }

        public TranslationResult Translate(string original)
            => string.IsNullOrWhiteSpace(original)
                ? TranslationResult.Empty
                : Compose(Decompose(original));

        public IDecomposition Decompose(string original)
        {
            var tokens = _tokenGenerator.GetTokens(original);
            var translationCandidates = Translate(tokens).ToArray();
            var possibilities = CompoundHandler.CompleteCompounds(translationCandidates, tokens).ToList();
            var undotted = RemoveRedundantDots(possibilities).ToArray();
            SetCodes(undotted);
            return new Decomposition(undotted);
        }

        public ReductionResult Reduce(IDecomposition decomposition) => _grammar.Reduce(decomposition);

        public TranslationResult Arrange(IDecomposition decomposition, ReductionResult reduction)
        {
            var arrangement = _arranger.Arrange(reduction.Grammatons).ToArray();
            var translations = SelectSynonyms(arrangement);
            var capitalized = _capitalizer.Capitalize(translations, reduction.Grammatons);
            var translation = Trim(capitalized);
            return new TranslationResult(translation, decomposition, reduction, arrangement, translations);
        }

        private static void SetCodes(IEnumerable<ITranslation[]> possibilities)
        {
            foreach (var alternatives in possibilities)
            foreach (var translation in alternatives)
                translation.Code = Encoder.Encode(translation.From);
        }

        private TranslationResult Compose(IDecomposition decomposition)
            => Arrange(decomposition, _grammar.Reduce(decomposition));

        private ITranslation[] SelectSynonyms(IGrammaton[] grammatons)
        {
            var retVal = new List<ITranslation>();
            var candidates = grammatons
                .Select(g => g.Translations).Append(new ITranslation[0])
                .ToArray();
            foreach (var pair in candidates.Skip(1).Select((next, i) => (next, i)))
                retVal.Add(_synonymResolver.Resolve(candidates[pair.i], retVal, pair.next));
            return retVal.ToArray();
        }

        private string Trim(IEnumerable<ITranslation> capitalized) => Merge(Respace(capitalized));

        private IEnumerable<ITranslation[]> Translate(IEnumerable<Token> tokens)
            => tokens.Select(_thesaurus.Translate);

        private static IEnumerable<ITranslation[]> RemoveRedundantDots(IEnumerable<ITranslation[]> possibilities)
        {
            ITranslation[] current = new ITranslation[0];
            foreach (var translations in possibilities)
            {
                var prev = current;
                current = translations;
                if (translations.Length == 1 && prev.Length == 1)
                {
                    var translation = translations.Single();
                    if (prev.Single().From is Abbreviation)
                    {
                        if (translation.From is Terminator)
                            continue;
                        if (translation.From is Ellipsis ellipsis)
                            ellipsis.Shortened = true;
                    }
                }
                yield return translations;
            }
        }

        private static IEnumerable<ITranslation> Respace(IEnumerable<ITranslation> translations)
        {
            var space = Translation.Create(new Divider());
            ITranslation previous = null;
            foreach (var translation in translations)
            {
                if (previous != null && !(translation.From is Punctuation || translation.From is Ellipsis))
                    yield return space;
                yield return previous = translation;
            }
        }

        private static string Merge(IEnumerable<ITranslation> translations)
            => string.Join("", translations.Select(translation => translation.Output)).Trim();
    }
}