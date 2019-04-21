using System;

namespace Lingua.Core.Tokens
{
    [Flags]
    public enum Modifier
    {
        None = 0,
        Plural = 1,
        Definite = 1 << 1,
        Genitive = 1 << 2,
        Neuter = 1 << 3,
        //Verb
        Passive = 1 << 1,
        Imperitive = 1 << 2,
        Continuous = 1 << 3,
        //Adjective
        Comparative = 1 << 4,
        Superlative = 1 << 5,
        Adverb = 1 << 6,
        //Verb+Pronoun
        FirstPerson = 1 << 4,
        SecondPerson = 1 << 5,
        ThirdPerson = 1 << 6,
        //Verb
        Past = 1 << 7,
        Perfect = 1 << 8,
        Future = 1 << 9,
        //Pronoun
        Object = 1 << 7,
        Possessive = 1 << 8,
        //Article
        Qualified = 1 << 9,
        Wildcard = 1 << 10
    }
}