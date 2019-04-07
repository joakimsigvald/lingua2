namespace Lingua.Grammar
{
    using Core;
    using Lingua.Core.Tokens;
    using Lingua.Core.WordClasses;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CodeCondenser
    {
        private static readonly IList<string> _nounPhrases = new[] { "T*N*", "T*A*N*" };

        private readonly Code[] _nounPhraseCodes = _nounPhrases.Select(Encoder.Encode).ToArray();

        public IEnumerable<ushort[]> GetReversedCodes(IEnumerable<ITranslation> translations)
        {
            ushort[] previousReversed = new ushort[] { Start.Code };
            return translations.Select(t => previousReversed = Condense(previousReversed, t));
        }

        public ushort[] Condense(ushort[] previousReversed, ITranslation next)
            => ReplaceLastNounPhrase(previousReversed).Prepend(next.Code).ToArray();

        private ushort[] ReplaceLastNounPhrase(ushort[] reversedCode)
        {
            var matchingNounPhrase = FindMatchingNounPhrase(reversedCode);
            return matchingNounPhrase is null
                ? reversedCode
                : ReplaceNounPhraseWithNoun(reversedCode, matchingNounPhrase!);
        }

        private Code? FindMatchingNounPhrase(ushort[] reversedCode)
            => _nounPhraseCodes.FirstOrDefault(np => StartsWith(reversedCode, np));

        private bool StartsWith(ushort[] reversedCode, Code nounPhrase)
            => nounPhrase.Length <= reversedCode.Length 
            && nounPhrase.ReversedCode
            .Select((c, i) => Encoder.Matches(reversedCode[i], c))
            .All(v => v);

        private ushort[] ReplaceNounPhraseWithNoun(ushort[] reversedCode, Code nounPhrase)
        {
            var newCode = new ushort[reversedCode.Length - nounPhrase.Length + 1];
            Array.Copy(reversedCode, nounPhrase.Length, newCode, 1, newCode.Length - 1);
            newCode[0] = Encoder.Recode<NounPhrase>(reversedCode[0]);
            return newCode;
        }
    }
}