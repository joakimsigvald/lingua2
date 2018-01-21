using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lingua.Core
{
    using Extensions;
    using Tokens;

    public class Translator : ITranslator
    {
        private readonly IThesaurus _thesaurus;
        private readonly IGrammar _grammar;
        private readonly TokenGenerator _tokenGenerator;

        public Translator(ITokenizer tokenizer, IThesaurus thesaurus, IGrammar grammar)
        {
            _thesaurus = thesaurus;
            _grammar = grammar;
            _tokenGenerator = new TokenGenerator(tokenizer);
        }

        public TranslationResult Translate(string original)
            => string.IsNullOrWhiteSpace(original)
                ? new TranslationResult("")
                : Compose(Decompose(original));

        public IList<ITranslation[]> Decompose(string original)
        {
            var tokens = _tokenGenerator.GetTokens(original);
            var translationCandidates = Translate(tokens).ToList();
            var possibilities = CompoundCombiner.Combine(translationCandidates).ToList();
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

            var recapitalized = PromoteInvisibleCapitalizations(arrangedTranslations, translations);
            var capitalized = CapitalizeStartOfSentences(recapitalized).ToArray();

            var respacedResult = Respace(capitalized).ToArray();
            var translation = Output(respacedResult);
            return new TranslationResult(translation)
            {
                Translations = translations,
                Reason = reason,
                Possibilities = possibilities
            };
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

        private IEnumerable<ITranslation[]> Translate(IReadOnlyList<Token> tokens)
            => Reduce(tokens.Select(_thesaurus.Translate), tokens);

        private static IEnumerable<ITranslation[]> Reduce(IEnumerable<ITranslation[]> alternatives,
            IReadOnlyList<Token> tokens)
            => alternatives.Select((candidates, ai) => Reduce(candidates, tokens, ai + 1).ToArray());

        private static IEnumerable<ITranslation> Reduce(IEnumerable<ITranslation> candidates,
            IReadOnlyList<Token> tokens,
            int nextIndex)
            => candidates.Where(t => t.Matches(tokens, nextIndex));

        private static IEnumerable<ITranslation> CapitalizeStartOfSentences(IEnumerable<ITranslation> translations)
            => SeparateSentences(translations).SelectMany(CapitalizeStartOfSentence);

        private static IEnumerable<IList<ITranslation>> SeparateSentences(IEnumerable<ITranslation> translations)
        {
            var nextSequence = new List<ITranslation>();
            foreach (var translation in translations)
            {
                nextSequence.Add(translation);
                if (!IsEndOfSentence(translation.From)) continue;
                yield return nextSequence;
                nextSequence = new List<ITranslation>();
            }
            yield return nextSequence;
        }

        private static IEnumerable<ITranslation> CapitalizeStartOfSentence(IList<ITranslation> sequence)
        {
            if (!IsSentence(sequence))
                return sequence;
            var preWord = sequence.TakeWhile(t => !(t.From is Element)).ToArray();
            var sentence = sequence.Skip(preWord.Length).ToArray();
            var firstWord = sentence.First();
            return firstWord.IsCapitalized
                ? sequence
                : preWord.Concat(sentence.Skip(1).Prepend(firstWord.Capitalize()));
        }

        private static bool IsSentence(IList<ITranslation> translations)
            => translations.Any(t => t.From is Element) && IsEndOfSentence(translations.Last().From);

        private static bool IsEndOfSentence(Token token)
            => token is Terminator || token is Ellipsis;

        private static IEnumerable<ITranslation> PromoteInvisibleCapitalizations(
            ICollection<ITranslation> arrangedTranslations, IList<ITranslation> allTranslations)
            => arrangedTranslations
                .Select(t => PromoteInvisibleCapitalization(
                    t, GetPreviousTranslation(allTranslations, t)));

        private static ITranslation GetPreviousTranslation(IList<ITranslation> allTranslations,
            ITranslation translation)
        {
            var index = allTranslations.IndexOf(translation);
            return index > 0 ? allTranslations[index - 1] : null;
        }

        private static ITranslation PromoteInvisibleCapitalization(
            ITranslation translation, ITranslation previousTranslation)
            => ShouldPromoteInvisibleCapitalization(translation, previousTranslation)
                ? translation.Capitalize()
                : translation;

        private static bool ShouldPromoteInvisibleCapitalization(
            ITranslation translation, ITranslation previousTranslation)
            => !translation.IsCapitalized
               && previousTranslation != null
               && IsInvisibleCapitalized(previousTranslation);

        private static bool IsInvisibleCapitalized(ITranslation previousWord)
            => previousWord.IsCapitalized && string.IsNullOrEmpty(previousWord.Output);

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

        private static readonly Regex Whitespace = new Regex(@"\s+");

        private static string Output(IEnumerable<ITranslation> translations)
            => Whitespace.Replace(string.Join("", translations
                    .Select(translation => translation.Output)).Trim()
                , " ");
    }
}