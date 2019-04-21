using System;
using Lingua.Core.WordClasses;

namespace Lingua.Core.Tokens
{
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
                case 1 << 2: 
                    switch (this)
                    {
                        case Verb _: return Modifier.Imperitive;
                        default: return Modifier.Genitive;
                    }
                case 1 << 3:
                    switch (this)
                    {
                        case Verb _: return Modifier.Continuous;
                        default: return Modifier.Neuter;
                    }
                case 1 << 4:
                    switch (this)
                    {
                        case Adjective _: return Modifier.Comparative;
                        default: return Modifier.FirstPerson;
                    }
                case 1 << 5:
                    switch (this)
                    {
                        case Adjective _: return Modifier.Superlative;
                        default: return Modifier.SecondPerson;
                    }
                case 1 << 6:
                    switch (this)
                    {
                        case Adjective _: return Modifier.Adverb;
                        default: return Modifier.ThirdPerson;
                    }
                case 1 << 7:
                    switch (this)
                    {
                        case Verb _: return Modifier.Past;
                        case Pronoun _: return Modifier.Object;
                        default: throw new NotImplementedException();
                    }
                case 1 << 8:
                    switch (this)
                    {
                        case Verb _: return Modifier.Perfect;
                        case Pronoun _: return Modifier.Possessive;
                        default: throw new NotImplementedException();
                    }
                case 1 << 9:
                    switch (this)
                    {
                        case Verb _: return Modifier.Future;
                        case Article _: return Modifier.Qualified;
                        default: throw new NotImplementedException();
                    }
                case 1 << 10:
                    return Modifier.Wildcard;
                default: throw new NotImplementedException();
            }
        }
    }
}