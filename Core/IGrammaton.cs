using System;

namespace Lingua.Core
{
    public interface IGrammaton : IEquatable<IGrammaton>
    {
        ITranslation[] Translations { get; }
        ushort Code { get; }
        byte WordCount { get; }
        string Input { get; }
        bool IsCapitalized { get; }
    }
}