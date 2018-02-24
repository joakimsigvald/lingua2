using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core
{
    using Tokens;

    public class Translator : ITranslator
    {
        private readonly IThesaurus _thesaurus;
        private readonly IGrammar _grammar;
        private readonly ICapitalizer _capitalizer;
        private readonly TokenGenerator _tokenGenerator;

        public Translator(ITokenizer tokenizer, IThesaurus thesaurus, IGrammar grammar, ICapitalizer capitalizer)
        {
            _thesaurus = thesaurus;
            _grammar = grammar;
            _tokenGenerator = new TokenGenerator(tokenizer);
            _capitalizer = capitalizer;
        }

        public TranslationResult Translate(string original)
            => string.IsNullOrWhiteSpace(original)
                ? new TranslationResult("")
                : Compose(Decompose(original));

        public IList<ITranslation[]> Decompose(string original)
        {
            var tokens = _tokenGenerator.GetTokens(original);
            var translationCandidates = Translate(tokens).ToArray();
            var possibilities = CompoundHandler.CompleteCompounds(translationCandidates, tokens).ToList();
            var undotted = RemoveRedundantDots(possibilities).ToArray();
            SetCodes(undotted);
            return undotted;
        }

        private static void SetCodes(IEnumerable<ITranslation[]> possibilities)
        {
            foreach (var alternatives in possibilities)
            foreach (var translation in alternatives)
                translation.Code = Encoder.Encode(translation.From);
        }

        public TranslationResult Compose(IList<ITranslation[]> possibilities)
        {
            (var translations, var reason) = _grammar.Reduce(possibilities);
            var arrangedTranslations = _grammar.Arrange(translations).ToList();
            var capitalized = _capitalizer.Capitalize(arrangedTranslations, translations);
            var respacedResult = Respace(capitalized).ToArray();
            var translation = Merge(respacedResult);
            return new TranslationResult(translation)
            {
                Translations = translations,
                Reason = reason,
                Possibilities = possibilities
            };
        }

        private IEnumerable<ITranslation[]> Translate(IEnumerable<Token> tokens)
            => tokens.Select(_thesaurus.Translate);

        private static IEnumerable<ITranslation[]> RemoveRedundantDots(IEnumerable<ITranslation[]> possibilities)
        {
            ITranslation[] current = null;
            foreach (var translations in possibilities)
            {
                var prev = current;
                current = translations;
                if (translations.Length == 1 && prev?.Length == 1)
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