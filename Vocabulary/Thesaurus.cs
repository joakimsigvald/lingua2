using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;

namespace Lingua.Vocabulary
{
    public class Thesaurus : IThesaurus
    {
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

        private static readonly IDictionary<string, string> Expanders = new Dictionary<string, string>
        {
            { "I'm", "I am"},
            { "he's", "he is"},
            { "she's", "she is"},
            { "it's", "it is"},
            { "they're", "they are"},
            { "you're", "you are"},
            { "we're", "we are"},
            { "who's", "who is"},
        };

        private static readonly IWordMap Quantifiers = new WordMap<Quantifier>
        {
            {"one", "en"},
            {"two", "två"},
            {"several", "flera"},
            {"many", "många"},
            {"all", "alla"},
        };

        private static readonly IWordMap Nouns = new WordMap<Noun>(GenitiveRules)
        {
            {"chair::s|", "stol:en:ar|na"},
            {"pen::s|", "penna:n:_or|na"},
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

        private static readonly IWordMap Articles = new WordMap<Article>
        {
            {"the:::<d", ":den::de"},
            {"a", "en"},
            {"an", "en"},
        };

        private static readonly IWordMap Prepositions = new WordMap<Preposition>
        {
            {"to", "till"},
            {"on", "på"},
            {"with", "med"},
            {"here", "här"},
        };

        private static readonly IWordMap Pronouns = new WordMap<Pronoun>
        {
            {"I!me:_y:_ine<1", "jag!mig:_n|"},
            {"we!us!our:s<1n", "vi!oss!vår|"},
            {"you::re|_s<2", "du:_ig:_in|"},
            {"you::re|_s<2n", "ni!er::"},
            {"he:_im:_is|<3", "han:_2onom:s|"},
            {"she!her:s|<3", "hon!henne:s|"},
            {"it::s|<3", "den::_ss|"},
            {"it::s|<3t", "det::_ss|"},
            {"they:_m:_ir|s<3n", "de:m:ras|"},
        };

        private static readonly IWordMap Adjectives = new WordMap<Adjective>
        {
            {"alright", "okej"},
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

        /// <summary>
        /// Imperative|Infinitive|Participle|First|Second|Third|ThirdPlural|Past|Perfect|PastContinuous|FutureContinuous
        /// </summary>
        private static readonly IWordMap Verbs = new WordMap<Verb>(baseForm: 1)
        {
            {"come::ing:::s::_3ame|:ing|", "kom:ma:mer||||::::ma"},
            {"help::ing:::s::ed|:ing|", "hjälp:a:er||||:te:::a"},
            {"paint::ing:::s::ed|:ing|", "måla::r||||:de:t|:"},
            {"play::ing:::s::ed|:ing|", "lek:a:er||||:te:t|:a"},
            {"run::ning:::s::_2an|:ning|", "spring:a:er||||:_3ang:_3ungit|:a"},
        };

        private static readonly IWordMap Auxiliaries = new WordMap<Auxiliary>(baseForm: 1)
        {
            {"be::ing!am!are!is!are!been!were", "var:a:_3är||||::it:"},
            {"have::_ing:::_2s::_2d:", "ha::r||||:de:ft:"},
            {"will::_ing:::::_3ould:_ing", "ska:::::::_ulle:_olat:_"},
            {"shall:::::::_3ould:", "ska:::::::_ulle:_olat:_"},
            {":will be<f", ":kommer att"},
            {":will have been<fr", ":kommer att ha"},
            {":could have been<fr", ":kunde ha"},
            {":going to<f", ":kommer att"},
            {":shall be<f", ":kommer att"},
            {":need to be<f", ":måste"},
        };

        private static readonly IWordMap InfinitiveMarkers = new WordMap<InfinitiveMarker>
        {
            {"to", "att"},
        };

        private static readonly IWordMap Conjunctions = new WordMap<Conjunction>
        {
            {"and", "och"},
        };

        private static readonly ILexicon Lexicon = new Lexicon(
            Abbreviations,
            Quantifiers,
            Nouns,
            Articles,
            Prepositions,
            Pronouns,
            Adjectives,
            Auxiliaries,
            Verbs,
            InfinitiveMarkers,
            Conjunctions
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

        public bool TryExpand(string word, out string expanded)
            => Expanders.TryGetValue(word, out expanded);

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