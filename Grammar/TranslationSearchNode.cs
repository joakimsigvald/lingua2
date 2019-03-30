namespace Lingua.Grammar
{
    using Core;
    using Lingua.Core.WordClasses;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class TranslationSearchNodeFactory
    {
        private static readonly IList<string> _nounPhrases = new[] { "TdNdt", "TdqANd" };

        public TranslationSearchNode Create(TranslationSearchNode parent, ITranslation translation, byte horizon)
            => new TranslationSearchNode {
                Translation = translation,
            WordCount = translation.WordCount,
            Index = (byte)(parent.Index + parent.WordCount),
            ReversedCode = ExtractReversedCode(parent, translation, horizon),
            Score = parent.Score
            };

        private ushort[] ExtractReversedCode(TranslationSearchNode parent, ITranslation translation, byte horizon)
            => ReplaceLastNounPhrase(parent.ReversedCode.Prepend(translation.Code).Take(horizon).ToArray());

        private ushort[] ReplaceLastNounPhrase(ushort[] code)
        {
            //return code;
            var matchingNounPhrase = FindMatchingNounPhrase(code);
            return matchingNounPhrase == null
                ? code
                : ReplaceNounPhraseWithNoun(code, matchingNounPhrase!);
        }

        private ushort[]? FindMatchingNounPhrase(ushort[] code)
            => _nounPhraseCodes.FirstOrDefault(np => StartsWith(code, np));

        private bool StartsWith(ushort[] code, ushort[] fragment)
            => fragment.Length <= code.Length && fragment.Select((c, i) => c == code[i]).All(v => v);

        private ushort[] ReplaceNounPhraseWithNoun(ushort[] code, ushort[] nounPhrase)
        {
            var newCode = new ushort[code.Length - nounPhrase.Length + 1];
            Array.Copy(code, nounPhrase.Length, newCode, 1, newCode.Length - 1);
            newCode[0] = Encoder.Recode<NounPhrase>(code[0]);
            return newCode;
        }

        private IList<ushort[]> _nounPhraseCodes =
            _nounPhrases
            .Select(np => Encoder.Encode(np).Reverse().ToArray())
            .ToArray();
    }

    internal class TranslationSearchNode
    {
        public static TranslationSearchNode[] NoChildren = new TranslationSearchNode[0];

        public ITranslation? Translation { get; internal set; }
        public TranslationSearchNode[] Children { get; set; } = NoChildren;
        public byte WordCount { get; internal set; }
        public int Score { get; set; }
        public int BestScore { get; set; } = int.MinValue;
        public byte Index { get; internal set; }
        public ushort[] ReversedCode { get; internal set; } = new ushort[0];
    }
}