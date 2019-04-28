using System.Collections.Generic;
using System.Linq;
using Lingua.Core.WordClasses;

namespace Lingua.Core
{
    using Tokens;

    public class Translator : ITranslator
    {
        private readonly IThesaurus _thesaurus;
        private readonly IGrammar _grammar;
        private readonly IArranger _arranger;
        private readonly ISynonymResolver _synonymResolver;
        private readonly ICapitalizer _capitalizer;
        private readonly TokenGenerator _tokenGenerator;

        public Translator(
            ITokenizer tokenizer, 
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
            _tokenGenerator = new TokenGenerator(tokenizer);
            _capitalizer = capitalizer;
        }

        public TranslationResult Translate(string original)
            => string.IsNullOrWhiteSpace(original)
                ? new TranslationResult("")
                : Compose(Decompose(original));

        public IList<IGrammaton[]> Decompose(string original)
        {
            var tokens = _tokenGenerator.GetTokens(original);
            var translationCandidates = Translate(tokens).ToArray();
            var possibilities = CompoundHandler.CompleteCompounds(translationCandidates, tokens).ToList();
            var undotted = RemoveRedundantDots(possibilities).ToArray();
            SetCodes(undotted);
            return undotted.Select(CreateGrammatons).ToArray();
        }

        private IGrammaton[] CreateGrammatons(ITranslation[] translations)
            => translations.GroupBy(t => (t.Code, t.WordCount)).Select(g => new Grammaton(g.ToArray())).ToArray();

        public ReductionResult Reduce(IList<IGrammaton[]> possibilities) => _grammar.Reduce(possibilities);

        public TranslationResult Arrange(IList<IGrammaton[]> possibilities, ReductionResult reduction)
        {
            var arrangement = _arranger.Arrange(reduction.Grammatons).ToArray();
            var translations = SelectSynonyms(arrangement);
            var capitalized = _capitalizer.Capitalize(translations, reduction.Grammatons);
            var translation = Trim(capitalized);
            return new TranslationResult(translation, possibilities, reduction, arrangement, translations);
        }

        private static void SetCodes(IEnumerable<ITranslation[]> possibilities)
        {
            foreach (var alternatives in possibilities)
            foreach (var translation in alternatives)
                translation.Code = Encoder.Encode(translation.From);
        }

        private TranslationResult Compose(IList<IGrammaton[]> possibilities)
            => Arrange(possibilities, _grammar.Reduce(possibilities));

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