using System;

namespace Lingua.Core.Tokens
{
    [Flags]
    public enum Modifier
    {
        None = 0, Definite = 1, Plural = 2, Possessive = 4, Qualified = 8
    }

    public abstract class Element : Token
    {
        public Modifier Modifiers { get; set; }
    }
}