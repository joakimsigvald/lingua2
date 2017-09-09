using System;
using Lingua.Core;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;
using NUnit.Framework;

namespace Lingua.Vocabulary.Test
{
    [TestFixture]
    public class ModificationRuleTest
    {
        [Test]
        public void CreatingRuleWithoutModifier_ThrowsException()
        {
            Assert.Throws<ArgumentException>(
                () => new ModificationRule<Noun>(Modifier.None, new string[0], new string[0]));
        }

        [Test]
        public void WhenWordClassNotMatch_ReturnsNull()
        {
            var verb = new Verb();
            var translation = Translation.Create(verb, "");
            var rule = new ModificationRule<Noun>(Modifier.Genitive, new[] { "*>*s" }, new[] { "*>*s" });
            var modification = rule.Apply(translation);
            Assert.That(modification, Is.Null);
        }

        [Test]
        public void EmptyRule_ReturnsNull()
        {
            var noun = new Noun();
            var translation = Translation.Create(noun, "");
            var rule = new ModificationRule<Noun>(Modifier.Genitive, new string[0], new string[0]);
            var modification = rule.Apply(translation);
            Assert.That(modification, Is.Null);
        }

        [Test]
        public void WhenHasFromIdentityTransformation_ConstructorThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new ModificationRule<Noun>(Modifier.Genitive, new[] { "*>*" }, new[] { "*>*" }));
        }

        [TestCase("a", "b")]
        [TestCase("s", "s")]
        [TestCase("'s", "s")]
        public void RuleWithSingleAddTransformation_ReturnsTransformedTranslation(string addToFrom, string addToTo)
        {
            const string from = "from";
            const string to = "to";
            var noun = new Noun
            {
                Value = from
            };
            var translation = Translation.Create(noun, to);
            var rule = new ModificationRule<Noun>(Modifier.Genitive, new[] { "*>*" + addToFrom }, new[] { "*>*" + addToTo });
            var modification = rule.Apply(translation);
            Assert.That(modification.From.Value, Is.EqualTo(from + addToFrom));
            Assert.That(modification.To, Is.EqualTo(to + addToTo));
        }

        [TestCase("abc", "c", true)]
        [TestCase("abc", "bc", true)]
        [TestCase("abc", "b", false)]
        public void CanMatchOnSuffix(string from, string suffix, bool matches)
        {
            var noun = new Noun
            {
                Value = from
            };
            var translation = Translation.Create(noun, "any");
            var rule = new ModificationRule<Noun>(Modifier.Genitive, new[] { $"*{suffix}>*s" }, new[] { "*>*s" });
            var modification = rule.Apply(translation);
            Assert.That(modification != null, Is.EqualTo(matches));
        }

        [TestCase("ball", "ball's")]
        [TestCase("balls", "balls'")]
        public void UsesMostSpecificMatchingFromTransform(string origin, string transformed)
        {
            var noun = new Noun
            {
                Value = origin
            };
            var translation = Translation.Create(noun, "any");
            var rule = new ModificationRule<Noun>(Modifier.Genitive, new[] { "*>*'s", "*s>*'" }, new[] { "*>*s" });
            var modification = rule.Apply(translation);
            Assert.That(modification.From.Value, Is.EqualTo(transformed));
        }

        [TestCase("boll", "bolls")]
        [TestCase("adress", "adress'")]
        public void UsesMostSpecificMatchingToTransform(string origin, string transformed)
        {
            var noun = new Noun
            {
                Value = "any"
            };
            var translation = Translation.Create(noun, origin);
            var rule = new ModificationRule<Noun>(Modifier.Genitive, new[] { "*>*s" }, new[] { "*s>*'", "*>*s" });
            var modification = rule.Apply(translation);
            Assert.That(modification.To, Is.EqualTo(transformed));
        }
    }
}