using System;
using Lingua.Grammar;
using Lingua.Tokenization;
using Lingua.Vocabulary;
using NUnit.Framework;

namespace Lingua.Core.Test
{
    [TestFixture]
    public class TranslatorTests
    {
        private static readonly ITranslator Translator
            = new Translator(new Tokenizer(), new Thesaurus(), new Engine(), new TestLogger());

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
        [TestCase("I run", "jag springer")]
        [TestCase("I run.", "Jag springer.")]
        [TestCase("I run!", "Jag springer!")]
        [TestCase("He runs. I run.", "Han springer. Jag springer.")]
        [TestCase("He runs! I run.", "Han springer! Jag springer.")]
        [TestCase("He runs... I run.", "Han springer... Jag springer.")]
        [TestCase("He runs. I run...", "Han springer. Jag springer...")]
        [TestCase("He runs, I run.", "Han springer, jag springer.")]
        [TestCase("He runs i.e. I run.", "Han springer dvs. jag springer.")]
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
        [TestCase("Play with the ball", "Lek med bollen")]
        [TestCase("the balls", "bollarna")]
        [TestCase("the streets", "gatorna")]
        [TestCase("Play with the balls", "Lek med bollarna")]
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
        [TestCase("I paint", "jag målar")]
        [TestCase("we paint", "vi målar")]
        [TestCase("you paint", "du målar")]
        [TestCase("he paints", "han målar")]
        [TestCase("she paints", "hon målar")]
        [TestCase("it paints", "den målar")]
        [TestCase("they paint", "de målar")]
        [TestCase("I paint the ball", "jag målar bollen")]
        [TestCase("He paints the ball", "Han målar bollen")]
        [TestCase("they paint the wall", "de målar väggen")]
        [TestCase("I am painting the wall", "jag målar väggen")]
        [TestCase("Are you painting?", "Målar du?")]
        [TestCase("Are you painting the wall?", "Målar du väggen?")]
        public void VerbsPresentTense(string from, string to)
            => Translates(from, to);

        [TestCase("I ran", "jag sprang")]
        [TestCase("He painted the wall", "Han målade väggen")]
        [TestCase("They ran fast", "De sprang snabbt")]
        public void VerbsPastTense(string from, string to)
            => Translates(from, to);

        [TestCase("I have ran", "jag har sprungit")]
        [TestCase("She has ran", "Hon har sprungit")]
        [TestCase("They have ran", "De har sprungit")]
        public void VerbsPerfectTense(string from, string to)
            => Translates(from, to);

        [TestCase("I had ran", "jag hade sprungit")]
        [TestCase("She had ran", "Hon hade sprungit")]
        [TestCase("They had ran", "De hade sprungit")]
        public void VerbsPastPerfectTense(string from, string to)
            => Translates(from, to);

        [TestCase("I will run", "jag ska springa")]
        [TestCase("I shall run", "jag ska springa")]
        [TestCase("I have been running", "jag har sprungit")]
        [TestCase("she has been running", "hon har sprungit")]
        [TestCase("I had been running", "jag hade sprungit")]
        [TestCase("I will be running", "jag kommer att springa")]
        [TestCase("I am going to run", "jag kommer att springa")]
        [TestCase("I will have been running", "jag kommer att ha sprungit")]
        [TestCase("I could have been running", "jag kunde ha sprungit")]
        [TestCase("he could have been running", "han kunde ha sprungit")]
        [TestCase("You could have been running", "Du kunde ha sprungit")]
        [TestCase("they could have been running", "de kunde ha sprungit")]
        public void VerbsMiscellaneous(string from, string to)
            => Translates(from, to);

        [TestCase("It is mine", "Den är min")]
        [TestCase("It is my pen", "Det är min penna")]
        [TestCase("Help me", "Hjälp mig")]
        [TestCase("I am here", "jag är här")]
        [TestCase("I am with her", "jag är med henne")]
        [TestCase("I'm with her", "jag är med henne")]
        [TestCase("She's with him", "Hon är med honom")]
        [TestCase("It's alright", "Det är okej")]
        public void Pronouns(string from, string to)
            => Translates(from, to);

        [TestCase("come to me", "kom till mig")]
        public void Prepositions(string from, string to)
            => Translates(from, to);

        private static void Translates(string from, string to)
            => Assert.That(Translator.Translate(from), Is.EqualTo(to));

        private class TestLogger : ILogger
        {
            public void Log(IReason reason) => reason.Evaluations.ForEach(Console.WriteLine);
        }
    }
}