﻿using Lingua.Core;
using System.Linq;

namespace Lingua.Translation
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

        public bool Equals(IGrammaton other)
            => other != null 
            && other.Code == Code 
            && other.WordCount == WordCount 
            && other.IsCapitalized == IsCapitalized;

        public override bool Equals(object obj)
            => obj is IGrammaton g && Equals(g);

        public override int GetHashCode() => Code;
    }
}