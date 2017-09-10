using System;

namespace Lingua.Core.Tokens
{
    [Flags]
    public enum Modifier
    {
        None = 0, Definite = 1, Plural = 2, Genitive = 4, Qualified = 8, ThirdPerson = 16, Present = 32,
        Comparative = 64, Superlative = 128
    }

    public abstract class Element : Token
    {
        public Modifier Modifiers { get; set; }
    }
}