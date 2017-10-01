using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Lingua.Core.Tokens;

namespace Lingua.Language
{
    public static class Utility
    {
        private const string BaseDir = "EnglishSwedish/Words/";

        private static readonly ILexicon Lexicon = new Lexicon(
            Language.Utility.GetWordMap<Abbreviation>(),
            Language.Utility.GetWordMap<Quantifier>(),
            Language.Utility.GetWordMap<Noun>(),
            Language.Utility.GetWordMap<Article>(),
            Language.Utility.GetWordMap<Preposition>(),
            Language.Utility.GetWordMap<Pronoun>(),
            Language.Utility.GetWordMap<Adjective>(),
            Language.Utility.GetWordMap<Auxiliary>(),
            Language.Utility.GetWordMap<Verb>(),
            Language.Utility.GetWordMap<InfinitiveMarker>(),
            Language.Utility.GetWordMap<Conjunction>(),
            Language.Utility.GetWordMap<Greeting>()
        );


        public static IWordMap GetWordMap<TWord>()
            where TWord : Word, new()
        {
            var allLines = ReadLines<TWord>();
            var wordLines = allLines
                .TakeWhile(line => line != "//Rules" && line != "//Settings")
                .ToArray();
            var ruleLines = allLines
                .Skip(wordLines.Length + 1)
                .TakeWhile(line => line != "//Settings")
                .ToArray();
            var settingsLines = allLines
                .Skip(wordLines.Length + ruleLines.Length + 2)
                .ToArray();
            var rules = ParseRules<TWord>(ruleLines);
            var settings = ParseSettings(settingsLines);
            var words = ParseWords(wordLines);
            return new WordMap<TWord>(words, rules.ToList(), settings.baseForm);
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
        {
            var fileName = $"{typeof(TWord).Name}s";
            var filePath = $"{BaseDir}{fileName}.txt";
            return File.ReadAllLines(filePath);
        }

        private static IDictionary<string, string> ParseWords(IEnumerable<string> wordLines)
        {
            var wordPairs = wordLines
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("//"))
                .Select(line => Regex.Split(line, " = "))
                .ToArray();
            return wordPairs.ToDictionary(pair => pair[0], pair => pair[1]);
        }

        private static IEnumerable<IModificationRule> ParseRules<TWord>(IList<string> ruleLines)
            where TWord : Word
        {
            var forLine = PickLine(ruleLines, "For");
            var fromLine = PickLine(ruleLines, "From");
            var toLine = PickLine(ruleLines, "To");
            if (forLine == null)
                yield break;
            Enum.TryParse<Modifier>(forLine.Split(':')[1], out var modifier);
            var fromTransforms = ParseTransforms(fromLine.Split(':')[1]);
            var toTransforms = ParseTransforms(toLine.Split(':')[1]);
            yield return new ModificationRule<TWord>(modifier, fromTransforms, toTransforms);
        }

        private static IEnumerable<string> ParseTransforms(string transformString)
            => transformString.Split(',');

        private static string PickLine(IEnumerable<string> lines, string label)
            => lines.SingleOrDefault(line => line.StartsWith(label));
    }
}