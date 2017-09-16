using System;

namespace Lingua.Core.Tokens
{
    [Flags]
    public enum Modifier
    {
        None = 0,
        Definite = 1, // double for Participle (-ing form)
        Plural = 2,
        Genitive = 4,
        Qualified = 8,
        FirstPerson = 16,
        SecondPerson = 32,
        ThirdPerson = 48,
        Comparative = 64,
        Superlative = 128,
        Past = 256
    }

    public abstract class Element : Token
    {
        public Modifier Modifiers { get; set; }
    }
}