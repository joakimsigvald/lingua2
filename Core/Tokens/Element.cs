using System;

namespace Lingua.Core.Tokens
{
    [Flags]
    public enum Modifier
    {
        None = 0,
        Definite = 1, // double for Participle (-ing form)
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
        Past = 1 << 10,
        Perfect = 1 << 11,
    }

    public abstract class Element : Token
    {
        public Modifier Modifiers { get; set; }
    }
}