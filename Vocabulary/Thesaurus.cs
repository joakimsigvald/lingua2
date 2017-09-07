using System.Linq;
using Lingua.Core;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;

namespace Lingua.Vocabulary
{
    public class Thesaurus : IThesaurus
    {
        private static readonly IWordMap Words = new WordMap<Unclassified>
        {
            {"and", "och"},
            {"my", "min"},
            {"to", "att"},
            {"with", "med"},
        };

        private static readonly IWordMap Abbreviations = new WordMap<Abbreviation>
        {
            {"i.e.", "dvs."},
            {"e.g.", "t.ex."}
        };

        private static readonly IWordMap Nouns = new WordMap<Noun>
        {
            {"ball|'s:|'s:s|':s|'", "boll|s:en|s:ar|s|_na|s"},
            {"foot|'s:|'s:___eet|'s|__|'s", "fot|s:en|s:__ötter|s|_na|s"},
            {"bouncing ball|'s:|'s:s|':s|'", "studsboll|s:en|s:ar|s|_na|s"},
            {"colour|'s:|'s:s|':s|'", "färg|s:en|s:er|s|_na|s"},
            {"search", "sök/"},
            {"result|'s:|'s:s|':s|'", "resultat|s:et|s:|s:en|s"},
            {"street|'s:|'s:s|':s|'", "gata|s:n|s:_or|s|_na|s/_u"},
            {"address|':|':es|'|_|'", "adress|:en|s:er|s|_na|s"},
        };

        private static readonly IWordMap Pronouns = new WordMap<Pronoun>
        {
            {"I", "jag"},
            {"he<3", "han"},
            {"they<3n", "de"},
        };

        private static readonly IWordMap Adjectives = new WordMap<Adjective>
        {
            {"red:::", "röd:a||"},
            {"good-looking", "snygg"},
            {"English-", "engelsk"},
            {"non-", "icke-"},
            {"French-", "fransk"},
            {"speaking", "talande"},
            {"easy", "lätt"},
            {"easy as pie", "lätt som en pannkaka"},
            {"easy peasy", "lätt som en pannkaka"},
            {"achievement-oriented", "prestationsorienterad"}
        };

        private static readonly IWordMap Verbs = new WordMap<Verb>
        {
            {"paint::s", "måla:r|"},
            {"play", "leka"},
        };

        private static readonly IWordMap Articles = new WordMap<Article>
        {
            {"the:::<d", ":den::de"},
            {"a", "en"},
            {"an", "en"},
        };

        private static readonly IWordMap Quantifiers = new WordMap<Quantifier>
        {
            {"one", "en"},
            {"two", "två"},
            {"several", "flera"},
            {"many", "många"},
            {"all", "alla"},
        };

        private static readonly ILexicon Lexicon = new Lexicon(
            Words, 
            Abbreviations, 
            Nouns,
            Pronouns,
            Adjectives,
            Verbs,
            Articles, 
            Quantifiers
            );

        public Translation[] Translate(Token token)
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

        private static Translation[] Translate(Generic generic)
            => Translate(generic.Stem).SelectMany(t => t.Variations).ToArray();

        private static Translation[] Translate(Word word)
        {
            var translations = Translate(word.Value);
            return !translations.Any() && word.PossibleAbbreviation
                ? Translate(word.Value + '.')
                : translations;
        }

        private static Translation[] Translate(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return new Translation[0];
            var exactTranslations = TranslateExact(word);
            return exactTranslations.Any()
                    ? exactTranslations
                    : TranslateCapitalized(word);
        }

        private static Translation[] TranslateCapitalized(string word)
            => TranslateExact(word.Decapitalize()).Select(t => t.Capitalize()).ToArray();

        private static Translation[] TranslateExact(string word)
        {
            var directTranslations = Lexicon.Lookup(word);
            return directTranslations.Any()
                ? directTranslations.ToArray()
                : TranslateParts(word);
        }

        private static Translation[] TranslateParts(string word)
        {
            var splitIndex = word.IndexOf('-') + 1;
            if (splitIndex < 2 || splitIndex >= word.Length)
                return new Translation[0];
            var firstWord = word.Substring(0, splitIndex);
            var firstTranslations = Lexicon.Lookup(firstWord)
                .Where(t => t.IsTranslatedWord)
                .ToList();
            if (!firstTranslations.Any())
                return new Translation[0];
            var remainingTranslations = TranslateExact(word.Substring(splitIndex))
                .Where(t => t.IsTranslatedWord)
                .ToList();
            return firstTranslations.SelectMany(ft => remainingTranslations.Select(rt =>
                    Translation.Create(new Unclassified {Value = ft.From.Value + rt.From.Value}, ft.To + rt.To)))
                .ToArray();
        }
    }
}