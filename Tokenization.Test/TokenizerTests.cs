using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core.WordClasses;
using Xunit;

namespace Lingua.Tokenization.Test
{
    using Core.Extensions;
    using Core.Tokens;

    public class TokenizerTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("  ")]
        public void Nothing(string nothing)
        {
            Assert.Empty(nothing.Tokenize());
        }

        [Theory]
        [InlineData(".")]
        [InlineData(" ?")]
        [InlineData("! ")]
        [InlineData("  : ")]
        public void Terminator(string terminator)
            => terminator.Tokenize().Yields<Terminator>($"{terminator.Trim()}");

        [Theory]
        [InlineData(",")]
        [InlineData("  ;     ")]
        public void Separator(string separator)
            => separator.Tokenize().Yields<Separator>($"{separator.Trim()}");

        [Theory]
        [InlineData("z")]
        [InlineData("e")]
        public void Letter(string letter)
            => letter.Tokenize().Yields<Word>(letter);

        [Theory]
        [InlineData("0")]
        [InlineData("1")]
        [InlineData("9")]
        public void Digit(string digit)
            => digit.Tokenize().Yields<Number>(digit);

        [Theory]
        [InlineData("hi")]
        [InlineData("you")]
        [InlineData("qwe")]
        [InlineData("rty")]
        [InlineData("I'm")]
        [InlineData("good-looking")]
        public void Word(string text)
            => text.Tokenize().Yields<Word>(text);

        [Theory]
        [InlineData("023")]
        [InlineData("9235235")]
        public void Number(string number)
            => number.Tokenize().Yields<Number>(number);

        [Theory]
        [InlineData("-23")]
        public void NegativeNumber(string number)
            => number.Tokenize().Yields<Number>(number);

        [Theory]
        [InlineData("0.23")]
        [InlineData("9235.235")]
        [InlineData("-9235.235")]
        public void DecimalNumber(string number)
            => number.Tokenize().Yields<Number>(number);

        [Theory]
        [InlineData("..")]
        [InlineData("...")]
        [InlineData(".....")]
        public void Ellipsis(string ellipsis)
            => ellipsis.Tokenize().Yields<Ellipsis>("...");

        [Theory]
        [InlineData("hey you", "hey", "you")]
        [InlineData("Where are you", "Where", "are", "you")]
        public void Words(string text, params string[] parts)
            => text.Tokenize().Yields(parts);

        [Theory]
        [InlineData("Where are you?", "Where", "are", "you", "?")]
        [InlineData("I am here  .", "I", "am", "here", ".")]
        [InlineData("Honey, I'm home.", "Honey", ",", "I'm", "home", ".")]
        [InlineData("Hey... you", "Hey", "...", "you")]
        [InlineData("  A    text; with spaces, dots and     a colon:  That's it! ", 
            "A", "text", ";", "with", "spaces", ",", "dots", "and", "a", "colon", ":", "That's", "it", "!")]
        public void Sentenses(string text, params string[] parts)
            => text.Tokenize().Yields(parts);

        [Theory]
        [InlineData("i.e.")]
        [InlineData("e.g.")]
        public void Abbreviation(string text)
        {
            var elements = text.Tokenize();
            var first = elements.First();
            Assert.True(first is Word);
            Assert.Equal(text.Start(-1), first.Value);
            Assert.True(((Word)first).PossibleAbbreviation);
            Assert.True(elements[1] is Terminator);
        }

        [Theory]
        [InlineData("i.e..")]
        [InlineData("e.g...")]
        [InlineData("e.g.....")]
        public void AbbreviationAndEllipsis(string text)
        {
            var elements = text.Tokenize();
            var first = elements.First();
            Assert.True(first is Word);
            Assert.Equal(text.TrimEnd('.'), first.Value);
            Assert.True(((Word)first).PossibleAbbreviation);
            Assert.True(elements[1] is Ellipsis);
        }

        [Theory]
        [InlineData("[[ball]]")]
        [InlineData("[[foot]]")]
        public void GenericWord(string text)
        {
            var tokens = text.Tokenize();
            Assert.Single(tokens);
            var generic = tokens[0] as Generic;
            Assert.NotNull(generic);
            Assert.Equal(text.Substring(2).Start(-2), generic.Stem.Value);
        }

        [Theory]
        [InlineData("[ball]")]
        [InlineData("[[[ball]]")]
        [InlineData("[[foot]]]")]
        public void InvalidGenericWord(string text)
        {
            Assert.Throws<NotImplementedException>(() => text.Tokenize());
        }

        [Theory]
        [InlineData("Joakim")]
        public void Name(string text)
        {
            var tokens = text.Tokenize();
            Assert.Equal(typeof(Name), tokens.Single().GetType());
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
            Assert.True(token is TToken);
            Assert.Equal(part, token.Value);
        }

        public static void Yields(this List<Token> tokens, params string[] parts)
        {
            Assert.Equal(parts.Length, tokens.Count);
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
                    Assert.True(token is Terminator);
                    Assert.Equal(representation, token.Value);
                    break;
                case ";":
                case ",":
                    Assert.True(token is Separator);
                    Assert.Equal(representation, token.Value);
                    break;
                case "...":
                    Assert.True(token is Ellipsis);
                    break;
                default:
                    Assert.True(token is Word);
                    Assert.Equal(representation, token.Value);
                    break;
            }
        }
    }
}