using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Lingua.Tokenization.Test
{
    using Core;
    using Core.Tokens;

    [TestFixture]
    public class TokenizerTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("  ")]
        public void Nothing(string nothing)
        {
            Assert.That(nothing.Tokenize(), Is.Empty);
        }

        [TestCase(".")]
        [TestCase(" ?")]
        [TestCase("! ")]
        [TestCase("  : ")]
        public void Terminator(string terminator)
            => terminator.Tokenize().Yields<Terminator>($"{terminator.Trim()}");

        [TestCase(",")]
        [TestCase("  ;     ")]
        public void Separator(string separator)
            => separator.Tokenize().Yields<Separator>($"{separator.Trim()}");

        [TestCase("z")]
        [TestCase("e")]
        public void Letter(string letter)
            => letter.Tokenize().Yields<Word>(letter);

        [TestCase("0")]
        [TestCase("1")]
        [TestCase("9")]
        public void Digit(string digit)
            => digit.Tokenize().Yields<Number>(digit);

        [TestCase("hi")]
        [TestCase("you")]
        [TestCase("qwe")]
        [TestCase("rty")]
        [TestCase("I'm")]
        [TestCase("good-looking")]
        public void Word(string text)
            => text.Tokenize().Yields<Word>(text);

        [TestCase("023")]
        [TestCase("9235235")]
        public void Number(string number)
            => number.Tokenize().Yields<Number>(number);

        [TestCase("-23")]
        public void NegativeNumber(string number)
            => number.Tokenize().Yields<Number>(number);

        [TestCase("0.23")]
        [TestCase("9235.235")]
        [TestCase("-9235.235")]
        public void DecimalNumber(string number)
            => number.Tokenize().Yields<Number>(number);

        [TestCase("..")]
        [TestCase("...")]
        [TestCase(".....")]
        public void Ellipsis(string ellipsis)
            => ellipsis.Tokenize().Yields<Ellipsis>("...");

        [TestCase("hey you", "hey", "you")]
        [TestCase("Where are you", "Where", "are", "you")]
        public void Words(string text, params string[] parts)
            => text.Tokenize().Yields(parts);

        [TestCase("Where are you?", "Where", "are", "you", "?")]
        [TestCase("I am here  .", "I", "am", "here", ".")]
        [TestCase("Honey, I'm home.", "Honey", ",", "I'm", "home", ".")]
        [TestCase("Hey... you", "Hey", "...", "you")]
        [TestCase("  A    text; with spaces, dots and     a colon:  That's it! ", 
            "A", "text", ";", "with", "spaces", ",", "dots", "and", "a", "colon", ":", "That's", "it", "!")]
        public void Sentenses(string text, params string[] parts)
            => text.Tokenize().Yields(parts);

        [TestCase("i.e.")]
        [TestCase("e.g.")]
        public void Abbreviation(string text)
        {
            var elements = text.Tokenize();
            var first = elements.First();
            Assert.That(first is Word);
            Assert.That(first.Value, Is.EqualTo(text.Start(-1)));
            Assert.That(((Word)first).PossibleAbbreviation);
            Assert.That(elements[1] is Terminator);
        }

        [TestCase("i.e..")]
        [TestCase("e.g...")]
        [TestCase("e.g.....")]
        public void AbbreviationAndEllipsis(string text)
        {
            var elements = text.Tokenize();
            var first = elements.First();
            Assert.That(first is Word);
            Assert.That(first.Value, Is.EqualTo(text.TrimEnd('.')));
            Assert.That(((Word)first).PossibleAbbreviation);
            Assert.That(elements[1] is Ellipsis);
        }

        [TestCase("[[ball]]")]
        [TestCase("[[foot]]")]
        public void GenericWord(string text)
        {
            var tokens = text.Tokenize();
            Assert.That(tokens.Count, Is.EqualTo(1));
            var generic = tokens[0] as Generic;
            Assert.That(generic, Is.Not.Null);
            Assert.That(generic.Stem.Value, Is.EqualTo(text.Substring(2).Start(-2)));
        }

        [TestCase("[ball]")]
        [TestCase("[[[ball]]")]
        [TestCase("[[foot]]]")]
        public void InvalidGenericWord(string text)
        {
            Assert.Throws<NotImplementedException>(() => text.Tokenize());
        }
    }

    internal static class Extensions
    {
        public static List<Token> Tokenize(this string text)
            => new Tokenizer().Tokenize(text).ToList();

        public static void Yields<TToken>(this IEnumerable<Token> tokens, string part)
            where TToken : Token
        {
            var token = tokens.Single();
            Assert.That(token is TToken);
            Assert.That(token.Value, Is.EqualTo(part));
        }

        public static void Yields(this List<Token> tokens, params string[] parts)
        {
            Assert.That(tokens.Count, Is.EqualTo(parts.Length));
            for (var i = 0; i < parts.Length; i++)
                tokens[i].Matches(parts[i]);
        }

        private static void Matches(this Token token, string representation)
        {
            switch (representation)
            {
                case ".":
                case ":":
                case "!":
                case "?":
                    Assert.That(token is Terminator);
                    Assert.That(token.Value, Is.EqualTo(representation));
                    break;
                case ";":
                case ",":
                    Assert.That(token is Separator);
                    Assert.That(token.Value, Is.EqualTo(representation));
                    break;
                case "...":
                    Assert.That(token is Ellipsis);
                    break;
                default:
                    Assert.That(token is Word);
                    Assert.That(token.Value == representation);
                    break;
            }
        }
    }
}