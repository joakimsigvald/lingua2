﻿using System.Linq;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    [TestFixture]
    public class TargetSelectorTests
    {
        [TestCase("I have been running", "jag har sprungit", "I", "have", "been", "running")]
        [TestCase("I will be running", "jag kommer att springa", "I", "will be", "running")]
        [TestCase("The balls' colour", "Bollarnas färg", "The", "balls'", "colour")]
        [TestCase("Bouncing ball to play with", "Studsboll att leka med", "Bouncing ball", "to", "play", "with")]
        public void SelectTranslations(string from, string to, params string[] expectedTranslations)
        {
            var translationResult = TestHelper.Translator.Translate(from);
            var filteredCandidates = TargetSelector
                .SelectTarget(translationResult.Possibilities, to)
                .Translations.Select(t => t.From.Value);
            Assert.That(filteredCandidates, Is.EquivalentTo(expectedTranslations));
        }

        [TestCase("I am running", "Jag springer", 1, 3)]
        [TestCase("He has been running", "Han har sprungit", 1, 2, 4)]
        [TestCase("Are you painting", "Målar du?", 3, 2)]
        [TestCase("I am painting the wall", "jag målar väggen", 1, 3, 4)] // the har ingen översättning
        public void ExtractOrdering(string from, string to, params int[] expected)
        {
            var translationResult = TestHelper.Translator.Translate(from);
            var target = TargetSelector.SelectTarget(translationResult.Possibilities, to);
            Assert.That(target.Order, Is.EquivalentTo(expected));
        }
    }
}