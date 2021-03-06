using Lingua.Core.Tokens;
using System.Collections.Generic;

namespace Lingua.Core
{
    public interface ITranslation
    {
        string Output { get; }
        Token From { get; }
        ushort Code { get; set; }
        byte WordCount { get; }
        bool IsCapitalized { get; }
        ITranslation[] Variations { get; set; }
        bool IsTranslatedWord { get;  }
        bool IsIncompleteCompound { get; set;  }
        string? To { get; }
        Word[] Continuation { get; }
        string Input { get; }
        ITranslation Capitalize();
        bool Matches(IReadOnlyList<Token> tokens, int nextIndex);
        ITranslation Decapitalize();
    }
}