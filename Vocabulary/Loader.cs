using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Lingua.Core;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;

namespace Lingua.Vocabulary
{
    public static class Loader
    {
        private const string WordsDir = "Words";

        public static ILexicon LoadLexicon()
        {
            var rules = LoadRules().ToArray();
            return new Lexicon(
                rules,
                Load<Abbreviation>(rules),
                Load<Quantifier>(rules),
                Load<Noun>(rules),
                Load<Article>(rules),
                Load<Preposition>(rules),
                Load<Pronoun>(rules),
                Load<Adjective>(rules),
                Load<Auxiliary>(rules),
                Load<Verb>(rules),
                Load<AdverbQualifying>(rules),
                Load<AdverbPositioning>(rules),
                Load<AdverbQuestion>(rules),
                Load<InfinitiveMarker>(rules),
                Load<Conjunction>(rules),
                Load<Greeting>(rules)
            );
        }

        private static IEnumerable<IModificationRule> LoadRules()
        {
            var ruleLines = LoaderBase.ReadFile(Path.Combine(WordsDir, "rules.txt"));
            return ParseRules(ruleLines);
        }

        private static IWordMap Load<TWord>(IModificationRule[] rules)
            where TWord : Word, new()
        {
            var allLines = ReadLines<TWord>();
            var wordLines = allLines
                .TakeWhile(line => line != "//Settings")
                .ToArray();
            var settingsLines = allLines
                .SkipWhile(line => line != "//Settings")
                .ToArray();
            var applicableRules = rules.Where(rule => rule.AppliesTo(typeof(TWord)));
            var settings = ParseSettings(settingsLines);
            var words = ParseWords(wordLines);
            return new WordMap<TWord>(words, applicableRules.ToList(), settings.baseForm);
        }

        private static (int baseForm, int placeholder) ParseSettings(IEnumerable<string> settingsLines)
        {
            var baseFormLine = PickLine(settingsLines, "BaseForm");
            if (baseFormLine == null)
                return (0, 0);
            var baseForm = int.Parse(Regex.Split(baseFormLine, " = ")[1].Trim());
            return (baseForm, 0);
        }

        private static string[] ReadLines<TWord>()
            => LoaderBase.ReadFile(Path.Combine(WordsDir, $"{typeof(TWord).Name}s.txt"));

        private static IDictionary<string, string> ParseWords(IEnumerable<string> wordLines)
        {
            var wordPairs = wordLines
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("//"))
                .Select(line => Regex.Split(line, " = "))
                .ToArray();
            return wordPairs.ToDictionary(pair => pair[0], pair => pair[1]);
        }

        private static IEnumerable<IModificationRule> ParseRules(IList<string> ruleLines)
        {
            var coversLine = PickLine(ruleLines, "Covers");
            var addsLine = PickLine(ruleLines, "Adds");
            var fromLine = PickLine(ruleLines, "From");
            var toLine = PickLine(ruleLines, "To");
            if (coversLine == null)
                return new IModificationRule[0];
            Enum.TryParse<Modifier>(GetValues(addsLine).Single(), out var modifier);
            var types = GetValues(coversLine).Select(GetWordType).ToArray();
            var fromTransforms = GetValues(fromLine);
            var toTransforms = GetValues(toLine);
            return new[] {new ModificationRule(types, modifier, fromTransforms, toTransforms)}
                .Concat(ParseRules(ruleLines.SkipWhile(line => !string.IsNullOrWhiteSpace(line)).ToList()));
        }

        private static Type GetWordType(string name)
            => typeof(Word).Assembly.GetTypes().Single(t => t.FullName == $"Lingua.Core.WordClasses.{name}");

        private static string[] GetValues(string line)
            => line.Split(':')[1].Split(',');

        private static string PickLine(IEnumerable<string> lines, string label)
            => lines.FirstOrDefault(line => line.StartsWith(label));
    }
}