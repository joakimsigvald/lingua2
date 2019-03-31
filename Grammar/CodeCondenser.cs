namespace Lingua.Grammar
{
    using Core;
    using Lingua.Core.WordClasses;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CodeCondenser
    {
        private static readonly IList<string> _nounPhrases = new[] { "TdNd", "TdNdt", "TdqAndNd", "TdqANdt" };

        public ushort[] ReplaceLastNounPhrase(ushort[] reversedCode)
        {
            var matchingNounPhrase = FindMatchingNounPhrase(reversedCode);
            return matchingNounPhrase == null
                ? reversedCode
                : ReplaceNounPhraseWithNoun(reversedCode, matchingNounPhrase!);
        }

        private ushort[]? FindMatchingNounPhrase(ushort[] reversedCode)
            => _nounPhraseCodes.FirstOrDefault(np => StartsWith(reversedCode, np));

        private bool StartsWith(ushort[] reversedCode, ushort[] fragment)
            => fragment.Length <= reversedCode.Length && fragment.Select((c, i) => c == reversedCode[i]).All(v => v);

        private ushort[] ReplaceNounPhraseWithNoun(ushort[] reversedCode, ushort[] nounPhrase)
        {
            var newCode = new ushort[reversedCode.Length - nounPhrase.Length + 1];
            Array.Copy(reversedCode, nounPhrase.Length, newCode, 1, newCode.Length - 1);
            newCode[0] = Encoder.Recode<NounPhrase>(reversedCode[0]);
            return newCode;
        }

        private ushort[][] _nounPhraseCodes =
            _nounPhrases
            .Select(np => Encoder.Encode(np).Reverse().ToArray())
            .ToArray();
    }
}