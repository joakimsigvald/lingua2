using Lingua.Grammar;
using Lingua.Tokenization;
using Lingua.Vocabulary;
using NUnit.Framework;

namespace Lingua.Core.Test
{
    [TestFixture]
    public class TranslatorTests
    {
        private static readonly ITranslator Translator = new Translator(new Tokenizer(), new Thesaurus(), new Engine());

        [TestCase(null, "")]
        [TestCase("", "")]
        [TestCase("  ", "")]
        [TestCase("Joakim", "Joakim")]
        [TestCase(" Joakim    Sigvald   ", "Joakim Sigvald")]
        public void Untranslatable(string from, string to)
            => Translates(from, to);

        [TestCase("a ball", "en boll")]
        [TestCase("my  foot", "min fot")]
        public void SimplePhrase(string from, string to)
            => Translates(from, to);

        [TestCase("bouncing ball", "studsboll")]
        [TestCase("Bouncing ball to play with", "Studsboll att leka med")]
        public void WordWithSpace(string from, string to)
            => Translates(from, to);

        [TestCase("A ball.", "En boll.")]
        [TestCase("A ball. ", "En boll.")]
        [TestCase("A ball .", "En boll.")]
        public void Sentence(string from, string to)
            => Translates(from, to);

        [TestCase("A ball. My foot.", "En boll. Min fot.")]
        [TestCase("A ball. My foot!", "En boll. Min fot!")]
        [TestCase("A ball? My foot!", "En boll? Min fot!")]
        [TestCase("A ball  ? My foot!", "En boll? Min fot!")]
        public void Sentences(string from, string to)
            => Translates(from, to);

        [TestCase("A ball i.e..", "En boll dvs...")]
        [TestCase("A ball e.g....", "En boll t.ex...")]
        public void AbbreviationWithEllipsis(string from, string to)
            => Translates(from, to);

        [TestCase("balls", "bollar")]
        [TestCase("feet", "fötter")]
        [TestCase("streets", "gator")]
        [TestCase("bouncing balls", "studsbollar")]
        public void Plural(string from, string to)
            => Translates(from, to);

        [TestCase("1 [[ball]]", "1 boll")]
        [TestCase("2 [[ball]]", "2 bollar")]
        [TestCase("0.5 [[ball]]", "0.5 bollar")]
        public void NumberToPlural(string from, string to)
            => Translates(from, to);

        [TestCase("the ball", "bollen")]
        [TestCase("Play with the ball", "Leka med bollen")]
        [TestCase("the balls", "bollarna")]
        [TestCase("the streets", "gatorna")]
        [TestCase("Play with the balls", "Leka med bollarna")]
        public void DefiniteNoun(string from, string to)
            => Translates(from, to);

        [TestCase("the ball's colour", "bollens färg")]
        [TestCase("a ball's colour", "en bolls färg")]
        [TestCase("The balls' colour", "Bollarnas färg")]
        [TestCase("2 balls' colour", "2 bollars färg")]
        [TestCase("the ball's colours", "bollens färger")]
        [TestCase("a ball's colours", "en bolls färger")]
        [TestCase("The balls' colours", "Bollarnas färger")]
        [TestCase("2 balls' colours", "2 bollars färger")]
        public void PossessiveNoun(string from, string to)
            => Translates(from, to);

        [TestCase("one ball", "en boll")]
        [TestCase("two balls", "två bollar")]
        [TestCase("many balls", "många bollar")]
        [TestCase("several balls", "flera bollar")]
        [TestCase("all streets", "alla gator")]
        public void Quantifiers(string from, string to)
            => Translates(from, to);

        [TestCase("search result", "sökresultat")]
        [TestCase("search results", "sökresultat")]
        [TestCase("the search result", "sökresultatet")]
        [TestCase("The search results", "Sökresultaten")]
        [TestCase("street address", "gatuadress")]
        public void DoubleNouns(string from, string to)
            => Translates(from, to);

        [TestCase("a red ball", "en röd boll")]
        [TestCase("The red ball", "Den röda bollen")]
        [TestCase("The red balls", "De röda bollarna")]
        [TestCase("red balls", "röda bollar")]
        [TestCase("Two red balls", "Två röda bollar")]
        [TestCase("a red ball", "en röd boll")]
        [TestCase("The two red balls", "De två röda bollarna")]
        [TestCase("a faster car", "en snabbare bil")]
        [TestCase("faster cars", "snabbare bilar")]
        [TestCase("The fastest car", "Den snabbaste bilen")]
        [TestCase("The fastest cars", "De snabbaste bilarna")]
        public void Adjectives(string from, string to)
            => Translates(from, to);

        [TestCase("paint the ball", "måla bollen")]
        [TestCase("I paint the ball", "jag målar bollen")]
        [TestCase("He paints the ball", "Han målar bollen")]
        [TestCase("they paint the ball", "de målar bollen")]
        public void Verbs(string from, string to)
            => Translates(from, to);

        private static void Translates(string from, string to)
            => Assert.That(Translator.Translate(from), Is.EqualTo(to));
    }
}