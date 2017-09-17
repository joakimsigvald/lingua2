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

        private static readonly IModificationRule[] GenitiveRules =
        {
            new ModificationRule<Noun>(Modifier.Genitive
                , new [] {"*>*'s", "*s>*'"}
                , new [] {"*>*s", "*s>*'"})
        };

        private static readonly IWordMap Nouns = new WordMap<Noun>(GenitiveRules)
        {
            {"ball::s|", "boll:en:ar|na"},
            {"wall::s|", "vägg:en:ar|na"},
            {"car::s|", "bil:en:ar|na"},
            {"foot::___eet|", "fot:en:__ötter|_na"},
            {"bouncing ball::s|", "studsboll:en:ar|na"},
            {"colour::s|", "färg:en:er|na"},
            {"search", "sök/"},
            {"result::s|", "resultat:et::en"},
            {"street::s|", "gata:n:_or|na/_u"},
            {"address::es|", "adress:en:er|_na"},
        };

        private static readonly IWordMap Pronouns = new WordMap<Pronoun>
        {
            {"I<1", "jag"},
            {"we<1n", "vi"},
            {"you<2", "du"},
            {"you<2n", "ni"},
            {"he<3", "han"},
            {"she<3", "hon"},
            {"it<3", "det"},
            {"they<3n", "de"},
        };

        private static readonly IWordMap Adjectives = new WordMap<Adjective>
        {
            {"red:::er:est|", "röd:_tt:a|re:ast|e"},
            {"fast:::er:est|:", "snabb:t:a|re:ast|e:t"},
            {"easy:::_ier|_st:_ily", "lätt::a|re:ast|e:"},
            {"good-looking", "snygg"},
            {"English-", "engelsk"},
            {"non-", "icke-"},
            {"French-", "fransk"},
            {"speaking", "talande"},
            {"easy as pie", "lätt som en pannkaka"},
            {"easy peasy", "lätt som en plätt"},
            {"achievement-oriented", "prestationsorienterad"}
        };

        private static readonly IWordMap Verbs = new WordMap<Verb>
        {
            {"paint:ing:::s::ed|:ing|", "måla:r||||:de:t|:"},
            {"play:ing:::s::ed|:ing|", "leka:_er||||:_te|_|:"},
            {"run:ning:::s::_2an|:ning|", "springa:_er||||:_4ang:_4ungit|:"},
        };

        private static readonly IWordMap Auxiliaries = new WordMap<Auxiliary>
        {
            {"be:ing!am!are!is!are!been!were", "vara:_4är||||:_|it:"},
            {"have:_ing:::_2s::_2d:", "ha:r||||:de:ft:"},
            {"will:_ing:::::_3ould:_ing", "ska::::::_ulle:_olat:_"},
            {"shall::::::_3ould:", "ska::::::_ulle:_olat:_"},
            {"will be<f", "kommer att"},
            {"will have been<fr", "kommer att ha"},
            {"could have been<fr", "kunde ha"},
            {"going to<f", "kommer att"},
            {"shall be<f", "kommer att"},
            {"need to be<f", "måste"},
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
            Quantifiers,
            Auxiliaries
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