using System;
using Lingua.Core;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;
using Xunit;

namespace Lingua.Vocabulary.Test
{
    public class ModificationRuleTest
    {
        [Fact]
        public void CreatingRuleWithoutModifier_ThrowsException()
        {
            Assert.Throws<ArgumentException>(
                () => new ModificationRule(new []{typeof(Noun) }, Modifier.None, new string[0], new string[0]));
        }

        [Fact]
        public void WhenWordClassNotMatch_ReturnsNull()
        {
            var verb = new Verb();
            var translation = Translation.Create(verb, "");
            var rule = new ModificationRule(new[] { typeof(Noun) }, Modifier.Genitive, new[] { "*>*s" }, new[] { "*>*s" });
            var modification = rule.Apply(translation);
            Assert.Null(modification);
        }

        [Fact]
        public void EmptyRule_ReturnsNull()
        {
            var noun = new Noun();
            var translation = Translation.Create(noun, "");
            var rule = new ModificationRule(new[] { typeof(Noun) }, Modifier.Genitive, new string[0], new string[0]);
            var modification = rule.Apply(translation);
            Assert.Null(modification);
        }

        [Fact]
        public void WhenHasFromIdentityTransformation_ConstructorThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new ModificationRule(new[] { typeof(Noun) }, Modifier.Genitive, new[] { "*>*" }, new[] { "*>*" }));
        }

        [Theory]
        [InlineData("a", "b")]
        [InlineData("s", "s")]
        [InlineData("'s", "s")]
        public void RuleWithSingleAddTransformation_ReturnsTransformedTranslation(string addToFrom, string addToTo)
        {
            const string from = "from";
            const string to = "to";
            var noun = new Noun
            {
                Value = from
            };
            var translation = Translation.Create(noun, to);
            var rule = new ModificationRule(new[] { typeof(Noun) }, Modifier.Genitive, new[] { "*>*" + addToFrom }, new[] { "*>*" + addToTo });
            var modification = rule.Apply(translation);
            Assert.Equal(from + addToFrom, modification.From.Value);
            Assert.Equal(to + addToTo, modification.To);
        }

        [Theory]
        [InlineData("abc", "c", true)]
        [InlineData("abc", "bc", true)]
        [InlineData("abc", "b", false)]
        public void CanMatchOnSuffix(string from, string suffix, bool matches)
        {
            var noun = new Noun
            {
                Value = from
            };
            var translation = Translation.Create(noun, "any");
            var rule = new ModificationRule(new[] { typeof(Noun) }, Modifier.Genitive, new[] { $"*{suffix}>*s" }, new[] { "*>*s" });
            var modification = rule.Apply(translation);
            Assert.Equal(matches, modification != null);
        }

        [Theory]
        [InlineData("ball", "ball's")]
        [InlineData("balls", "balls'")]
        public void UsesMostSpecificMatchingFromTransform(string origin, string transformed)
        {
            var noun = new Noun
            {
                Value = origin
            };
            var translation = Translation.Create(noun, "any");
            var rule = new ModificationRule(new[] { typeof(Noun) }, Modifier.Genitive, new[] { "*>*'s", "*s>*'" }, new[] { "*>*s" });
            var modification = rule.Apply(translation);
            Assert.Equal(transformed, modification.From.Value);
        }

        [Theory]
        [InlineData("boll", "bolls")]
        [InlineData("adress", "adress'")]
        public void UsesMostSpecificMatchingToTransform(string origin, string transformed)
        {
            var noun = new Noun
            {
                Value = "any"
            };
            var translation = Translation.Create(noun, origin);
            var rule = new ModificationRule(new[] { typeof(Noun) }, Modifier.Genitive, new[] { "*>*s" }, new[] { "*s>*'", "*>*s" });
            var modification = rule.Apply(translation);
            Assert.Equal(transformed, modification.To);
        }
    }
}