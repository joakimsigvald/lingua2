using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lingua.Core
{
    using Extensions;
    using Tokens;
    using WordClasses;

    public class Translator : ITranslator
    {
        private readonly ITokenizer _tokenizer;
        private readonly IThesaurus _thesaurus;
        private readonly IGrammar _grammar;

        public Translator(ITokenizer tokenizer, IThesaurus thesaurus, IGrammar grammar)
        {
            _tokenizer = tokenizer;
            _thesaurus = thesaurus;
            _grammar = grammar;
        }

        public TranslationResult Translate(string original)
        {
            if (string.IsNullOrWhiteSpace(original))
                return new TranslationResult
                {
                    Translation = string.Empty
                };
            var possibilities = Destruct(original);
            return Construct(possibilities);
        }

        public IList<ITranslation[]> Destruct(string original)
        {
            var tokens = Expand(Tokenize(original)).ToArray();
            var possibilities = CompoundCombiner.Combine(Translate(tokens).ToList()).ToList();
            SetCodes(possibilities);
            return possibilities;
        }

        private static void SetCodes(IEnumerable<ITranslation[]> possibilities)
        {
            foreach (var alternatives in possibilities)
            foreach (var translation in alternatives)
                translation.Code = Encoder.Encode(translation.From);
        }

        public TranslationResult Construct(IList<ITranslation[]> possibilities)
        {
            (var translations, var reason) = _grammar.Reduce(possibilities);
            var arrangedTranslations = _grammar.Arrange(translations).ToList();

            var recapitalized = PromoteInvisibleCapitalizations(arrangedTranslations, translations);
            var undotted = RemoveRedundantDots(recapitalized).ToArray();
            var capitalized = CapitalizeStartOfSentences(undotted).ToArray();

            var respacedResult = Respace(capitalized).ToArray();
            var translation = Output(respacedResult);
            return new TranslationResult
            {
                Translations = translations,
                Translation = translation,
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

        private IEnumerable<Token> Expand(IEnumerable<Token> tokens)
            => tokens.SelectMany(Expand);

        private IEnumerable<Token> Expand(Token token)
            => Tokenize(Expand(token as Unclassified)) ?? new[] {token};

        private string Expand(Unclassified word)
            => word == null
                ? null
                : _thesaurus.TryExpand(word.Value, out string exactExpanded)
                    ? exactExpanded
                    : _thesaurus.TryExpand(word.Value.ToLower(), out string lowerExpanded)
                        ? lowerExpanded.Capitalize()
                        : null;

        private IEnumerable<Token> Tokenize(string text)
            => text == null ? null : _tokenizer.Tokenize(text);

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
                    arrangedTranslations, t, GetPreviousTranslation(allTranslations, t)));

        private static ITranslation GetPreviousTranslation(IList<ITranslation> allTranslations,
            ITranslation translation)
        {
            var index = allTranslations.IndexOf(translation);
            return index > 0 ? allTranslations[index - 1] : null;
        }

        private static ITranslation PromoteInvisibleCapitalization(
            ICollection<ITranslation> arrangedTranslations
            , ITranslation translation
            , ITranslation previousTranslation)
            => ShouldPromoteInvisibleCapitalization(arrangedTranslations, translation, previousTranslation)
                ? translation.Capitalize()
                : translation;

        private static bool ShouldPromoteInvisibleCapitalization(
            ICollection<ITranslation> arrangedTranslations
            , ITranslation translation
            , ITranslation previousTranslation)
            => !translation.IsCapitalized
               && previousTranslation != null
               && IsInvisibleCapitalized(arrangedTranslations, previousTranslation);

        private static bool IsInvisibleCapitalized(
            ICollection<ITranslation> arrangedTranslations, ITranslation previousWord)
            => previousWord.IsCapitalized 
            && (string.IsNullOrEmpty(previousWord.Output) || !arrangedTranslations.Contains(previousWord));

        private static IEnumerable<ITranslation> RemoveRedundantDots(IEnumerable<ITranslation> translations)
        {
            ITranslation current = null;
            foreach (var translation in translations)
            {
                var prev = current;
                current = translation;
                if (prev?.From is Abbreviation)
                {
                    if (translation.From is Terminator)
                        continue;
                    if (translation.From is Ellipsis ellipsis)
                        ellipsis.Shortened = true;
                }
                yield return translation;
            }
        }

        private static readonly Regex Whitespace = new Regex(@"\s+");

        private static string Output(IEnumerable<ITranslation> translations)
            => Whitespace.Replace(string.Join("", translations
                    .Select(translation => translation.Output)).Trim()
                , " ");
    }
}