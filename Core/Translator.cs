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

        public TranslationTreeNode Destruct(string original)
        {
            var tokens = Expand(Tokenize(original)).ToArray();
            var candidates = Translate(tokens).ToList();
            var completedCandidates = CompoundCombiner.Combine(candidates).ToList();
            return new TranslationTreeNode(null, completedCandidates);
        }

        public TranslationResult Construct(TranslationTreeNode possibilities)
        {
            (var translations, var reason) = _grammar.Reduce(possibilities);
            var arrangedTranslations = _grammar.Arrange(translations);
            var adjustedResult = Adjust(arrangedTranslations).ToArray();
            var respacedResult = Respace(adjustedResult).ToArray();
            var translation = Output(respacedResult);
            return new TranslationResult
            {
                Translations = translations,
                Translation = translation,
                Reason = reason,
                Possibilities = possibilities
            };
        }

        private static IEnumerable<Translation> Respace(IEnumerable<Translation> translations)
        {
            var space = Translation.Create(new Divider());
            Translation previous = null;
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
            => Tokenize(Expand(token as Unclassified)) ?? new[] { token };

        private string Expand(Unclassified word)
            => word == null ? null 
            : _thesaurus.TryExpand(word.Value, out string exactExpanded)
                ? exactExpanded
                : _thesaurus.TryExpand(word.Value.ToLower(), out string lowerExpanded)
                    ? lowerExpanded.Capitalize()
                    : null;

        private IEnumerable<Token> Tokenize(string text)
            => text == null ? null : _tokenizer.Tokenize(text);

        private IEnumerable<Translation[]> Translate(IReadOnlyList<Token> tokens)
            => Reduce(tokens.Select(_thesaurus.Translate), tokens);

        private static IEnumerable<Translation[]> Reduce(IEnumerable<Translation[]> alternatives,
            IReadOnlyList<Token> tokens)
            => alternatives.Select((candidates, ai) => Reduce(candidates, tokens, ai + 1).ToArray());

        private static IEnumerable<Translation> Reduce(IEnumerable<Translation> candidates, IReadOnlyList<Token> tokens,
            int nextIndex)
            => candidates.Where(t => t.Matches(tokens, nextIndex));

        private static IEnumerable<Translation> Adjust(IEnumerable<Translation> translations)
            => PromoteInvisibleCapitalization(
                CapitalizeStartOfSentences(
                    RemoveRedundantDots(translations)));

        private static IEnumerable<Translation> CapitalizeStartOfSentences(IEnumerable<Translation> translations)
            => SeparateSentences(translations).SelectMany(CapitalizeStartOfSentence);

        private static IEnumerable<IList<Translation>> SeparateSentences(IEnumerable<Translation> translations)
        {
            var nextSequence = new List<Translation>();
            foreach (var translation in translations)
            {
                nextSequence.Add(translation);
                if (!IsEndOfSentence(translation.From)) continue;
                yield return nextSequence;
                nextSequence = new List<Translation>();
            }
            yield return nextSequence;
        }

        private static IEnumerable<Translation> CapitalizeStartOfSentence(IList<Translation> sequence)
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

        private static bool IsSentence(IList<Translation> translations)
            => translations.Any(t => t.From is Element) && IsEndOfSentence(translations.Last().From);

        private static bool IsEndOfSentence(Token token)
            => token is Terminator || token is Ellipsis;

        private static IEnumerable<Translation> PromoteInvisibleCapitalization(IEnumerable<Translation> translations)
        {
            Translation prevWord = null;
            foreach (var translation in translations)
            {
                yield return prevWord?.IsInvisibleCapitalized ?? false
                    ? translation.Capitalize()
                    : translation;
                if (translation.From is Word)
                    prevWord = translation;
            }
        }

        private static IEnumerable<Translation> RemoveRedundantDots(IEnumerable<Translation> translations)
        {
            Translation current = null;
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

        private static string Output(IEnumerable<Translation> translations)
            => Whitespace.Replace(string.Join("", translations
                    .Select(translation => translation.Output)).Trim()
                , " ");
    }
}