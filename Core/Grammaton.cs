using Lingua.Core.Tokens;
using System.Linq;

namespace Lingua.Core
{
    public class Grammaton : IGrammaton
    {
        public Grammaton(params ITranslation[] translations)
        {
            Translations = translations;
            Code = translations.First().Code;
            WordCount = translations.First().WordCount;
            Input = translations.First().Input;
            IsCapitalized = translations.First().IsCapitalized;
        }

        public ITranslation[] Translations { get; }
        public ushort Code { get; }
        public byte WordCount { get; }
        public string Input { get; set; }
        public bool IsCapitalized { get; set; }
    }
}