using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;

namespace Lingua.Vocabulary
{
    public static class Loader
    {
        private const string BaseDir = "Languages";
        private const string LanguageDir = "EnglishSwedish";
        private const string WordsDir = "Words";

        public static ILexicon LoadLexicon() => new Lexicon(
            Load<Abbreviation>(),
            Load<Quantifier>(),
            Load<Noun>(),
            Load<Article>(),
            Load<Preposition>(),
            Load<Pronoun>(),
            Load<Adjective>(),
            Load<Auxiliary>(),
            Load<Verb>(),
            Load<InfinitiveMarker>(),
            Load<Conjunction>(),
            Load<Greeting>()
        );

        private static IWordMap Load<TWord>()
            where TWord : Word, new()
        {
            var allLines = ReadLines<TWord>();
            var wordLines = allLines
                .TakeWhile(line => line != "//Rules" && line != "//Settings")
                .ToArray();
            var ruleLines = allLines
                .SkipWhile(line => line != "//Rules")
                .TakeWhile(line => line != "//Settings")
                .ToArray();
            var settingsLines = allLines
                .SkipWhile(line => line != "//Settings")
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
            => ReadFile(Path.Combine(WordsDir, $"{typeof(TWord).Name}s.txt"));

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

        public static IDictionary<string, string> LoadExpanders()
        {
            var lines = ReadExpanerLines();
            return lines.Select(ParseExpander)
                .ToDictionary(expander => expander.from, expander => expander.to);
        }

        private static (string from, string to) ParseExpander(string line)
        {
            var parts = Regex.Split(line, " => ");
            return (parts[0].Trim(), parts[1].Trim());
        }

        private static string[] ReadExpanerLines()
            => ReadFile("Expanders.txt");

        private static string[] ReadFile(string relativePath)
        { 
            var projectDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var solutionDir = projectDir.Parent.Parent.Parent;
            var filePath = Path.Combine(solutionDir.FullName, BaseDir, LanguageDir, relativePath);
            return File.ReadAllLines(filePath);
        }
    }
}