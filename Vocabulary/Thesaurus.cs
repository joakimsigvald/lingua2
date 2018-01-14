using System.Linq;

namespace Lingua.Vocabulary
{
    using Core;
    using Core.Extensions;
    using Core.Tokens;
    using Core.WordClasses;

    public class Thesaurus : IThesaurus
    {
        private static readonly ILexicon Lexicon = Loader.LoadLexicon();

        public ITranslation[] Translate(Token token)
        {
            if (token is Generic generic)
                return Translate(generic);
            var translations = token is Word word
                ? Translate(word)
                : new Translation[0];
            return translations.Any()
                ? translations.ToArray()
                : new[] { Translation.Create(token)};
        }

        private static ITranslation[] Translate(Generic generic)
            => Translate(generic.Stem).SelectMany(t => t.Variations).ToArray();

        private static ITranslation[] Translate(Word word)
        {
            var translations = Translate(word.Value);
            return !translations.Any() && word.PossibleAbbreviation
                ? Translate(word.Value + '.')
                : translations;
        }

        private static ITranslation[] Translate(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return new ITranslation[0];
            var exactTranslations = TranslateExact(word);
            return exactTranslations.Any()
                    ? exactTranslations
                    : TranslateCapitalized(word);
        }

        private static ITranslation[] TranslateCapitalized(string word)
            => TranslateExact(word.Decapitalize()).Select(t => t.Capitalize()).ToArray();

        private static ITranslation[] TranslateExact(string word)
        {
            var directTranslations = Lexicon.Lookup(word);
            return directTranslations.Any()
                ? directTranslations.ToArray()
                : TranslateParts(word);
        }

        private static ITranslation[] TranslateParts(string word)
        {
            var splitIndex = word.IndexOf('-') + 1;
            if (splitIndex < 2 || splitIndex >= word.Length)
                return new ITranslation[0];
            var firstWord = word.Substring(0, splitIndex);
            var firstTranslations = Lexicon.Lookup(firstWord)
                .Where(t => t.IsTranslatedWord)
                .ToList();
            if (!firstTranslations.Any())
                return new ITranslation[0];
            var remainingTranslations = TranslateExact(word.Substring(splitIndex))
                .Where(t => t.IsTranslatedWord)
                .ToList();
            return firstTranslations.SelectMany(ft => remainingTranslations.Select(rt =>
                    Translation.Create(new Unclassified {Value = ft.From.Value + rt.From.Value}, ft.To + rt.To)))
                .ToArray();
        }
    }
}