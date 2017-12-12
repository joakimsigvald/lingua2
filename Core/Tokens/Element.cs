using System;
using Lingua.Core.WordClasses;

namespace Lingua.Core.Tokens
{
    [Flags]
    public enum Modifier
    {
        None = 0,
        //Common
        Plural = 1,
        //Noun+Adjective
        Definite = 1 << 1,
        //Noun
        Genitive = 1 << 2,
        //Adjective+Pronoun+Noun
        Neuter = 1 << 3,
        //Adjective
        Comparative = 1 << 4,
        Superlative = 1 << 5,
        Adverb = 1 << 6,
        //Verb
        Imperitive = 1 << 3,
        Participle = 1 << 4,
        Past = 1 << 8,
        Perfect = 1 << 9,
        Future = 1 << 10,
        //Verb+Pronoun
        FirstPerson = 1 << 5,
        SecondPerson = 1 << 6,
        ThirdPerson = 1 << 7,
        //Article
        Qualified = 1 << 8,
        //Pronoun
        Object = 1 << 9,
        Possessive = 1 << 10,
        Any = Encoder.ModifiersMask
    }

    public abstract class Element : Token
    {
        public Modifier Modifiers { get; set; }

        public Modifier DecodeModifier(ushort code)
        {
            switch (code)
            {
                case 0: return Modifier.None;
                case 1: return Modifier.Plural;
                case 1 << 1: return Modifier.Definite;
                case 1 << 2: return Modifier.Genitive;
                case 1 << 3:
                    switch (this)
                    {
                        case Verb _: return Modifier.Imperitive;
                        default: return Modifier.Neuter;
                    }
                case 1 << 4:
                    switch (this)
                    {
                        case Adjective _: return Modifier.Comparative;
                        case Verb _: return Modifier.Participle;
                        default: throw new NotImplementedException();
                    }
                case 1 << 5:
                    switch (this)
                    {
                        case Adjective _: return Modifier.Superlative;
                        case Pronoun _:
                        case Verb _: return Modifier.FirstPerson;
                        default: throw new NotImplementedException();
                    }
                case 1 << 6:
                    switch (this)
                    {
                        case Adjective _: return Modifier.Adverb;
                        case Pronoun _:
                        case Verb _: return Modifier.SecondPerson;
                        default: throw new NotImplementedException();
                    }
                case 1 << 7: return Modifier.ThirdPerson;
                case 1 << 8:
                    switch (this)
                    {
                        case Verb _: return Modifier.Past;
                        case Article _: return Modifier.Qualified;
                        default: throw new NotImplementedException();
                    }
                case 1 << 9:
                    switch (this)
                    {
                        case Verb _: return Modifier.Perfect;
                        case Pronoun _: return Modifier.Object;
                        default: throw new NotImplementedException();
                    }
                case 1 << 10:
                    switch (this)
                    {
                        case Verb _: return Modifier.Future;
                        case Pronoun _: return Modifier.Possessive;
                        default: throw new NotImplementedException();
                    }
                case 0x03ff: return Modifier.Any;
                default: throw new NotImplementedException();
            }
        }
    }
}