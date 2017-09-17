using System;

namespace Lingua.Core.Tokens
{
    [Flags]
    public enum Modifier
    {
        None = 0,
        Definite = 1, // double for Verb Participle (-ing form)
        Plural = 1 << 1,
        Genitive = 1 << 2,
        Qualified = 1 << 3,
        FirstPerson = 1 << 4,
        SecondPerson = 1 << 5,
        ThirdPerson = 3 << 4,
        Comparative = 1 << 6,
        Superlative = 1 << 7,
        Neuter = 1 << 8,
        Adverb = 1 << 9,
        Imperitive = 1 << 10,
        Past = 1 << 11,
        Perfect = 1 << 12,
        Future = 1 << 13,
        Object = 1 << 14,
        Possessive = 1 << 15,
        Any = 0xffff
    }

    public abstract class Element : Token
    {
        public Modifier Modifiers { get; set; }
    }
}