using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lingua.Core.Tokens;

namespace Lingua.Core
{
    public class Translator : ITranslator
    {
        private readonly ITokenizer _tokenizer;
        private readonly IThesaurus _thesaurus;
        private readonly IGrammar _grammar;

        public Translator(ITokenizer tokenizer, IThesaurus thesaurus, IGrammar grammar)
        {
            _tokenizer = tokenizer;
            _thesaurus = thesaurus;
            _grammar = grammar;
        }

        public string Translate(string original)
            => Output(
                Adjust(
                    _grammar.Reduce(
                        Combine(
                            Translate(
                                _tokenizer.Tokenize(original)
                                .ToArray())
                            .ToArray()))));

        private IEnumerable<Translation[]> Translate(IReadOnlyList<Token> tokens)
            => Reduce(tokens.Select(_thesaurus.Translate), tokens);

        private static IEnumerable<Translation[]> Reduce(IEnumerable<Translation[]> alternatives, IReadOnlyList<Token> tokens)
            => alternatives.Select((candidates, ai) => Reduce(candidates, tokens, ai + 1).ToArray());

        private static IEnumerable<Translation> Reduce(IEnumerable<Translation> candidates, IReadOnlyList<Token> tokens, int nextIndex)
            => candidates.Where(t => t.Matches(tokens, nextIndex));

        private static IList<TreeNode<Translation>> Combine(IReadOnlyList<Translation[]> alternatives)
            => alternatives.Any()
            ? alternatives.First()
                .Select(t => CreateTreeNode(t, alternatives.Skip(t.TokenCount).ToList()))
                .ToList()
            : new List<TreeNode<Translation>>();

        private static TreeNode<Translation> CreateTreeNode(Translation translation, IReadOnlyList<Translation[]> future)
            => new TreeNode<Translation>(translation, () => Combine(future));

        private static IEnumerable<Translation> Adjust(IEnumerable<Translation> translations)
            => PromoteInvisibleCapitalization(
                RemoveRedundantDots(translations));

        private static IEnumerable<Translation> PromoteInvisibleCapitalization(IEnumerable<Translation> translations)
        {
            Translation prevWord = null;
            foreach (var translation in translations)
            {
                yield return prevWord?.IsInvisibleCapitalized ?? false
                    ? translation.Capitalize()
                    : translation;
                if (translation.From is Word)
                    prevWord = translation;
            }
        }

        private static IEnumerable<Translation> RemoveRedundantDots(IEnumerable<Translation> translations)
        {
            Translation current = null;
            foreach (var translation in translations)
            {
                var prev = current;
                current = translation;
                if (prev?.From is Abbreviation)
                {
                    if (translation.From is Terminator)
                        continue;
                    if (translation.From is Ellipsis ellipsis)
                        ellipsis.Shortened = true;
                }
                yield return translation;
            }
        }

        private static readonly Regex Whitespace = new Regex(@"\s+");

        private static string Output(IEnumerable<Translation> translations)
            => Whitespace.Replace(string.Join("", translations
                .Select(translation => translation.Output)).Trim()
            , " ");
    }
}