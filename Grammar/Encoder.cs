﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    using Core.Tokens;
    using Core.WordClasses;

    public static class Encoder
    {
        public static IEnumerable<int> Code(IEnumerable<Token> tokens)
            => tokens.Where(t => !(t is Divider)).Select(Code);

        public static string Serialize(IEnumerable<Token> tokens)
            => string.Join("", tokens.Where(t => !(t is Divider)).Select(Serialize));

        public static IEnumerable<Token> Deserialize(string serial)
        {
            if (string.IsNullOrEmpty(serial))
                yield break;

            var modifiers = "";
            var primary = serial[0];
            foreach (var c in serial.Skip(1))
            {
                if (char.IsLower(c))
                    modifiers += c;
                else
                {
                    yield return CreateToken(primary, modifiers);
                    primary = c;
                    modifiers = "";
                }
            }
            yield return CreateToken(primary, modifiers);
        }

        private static string Serialize(Token token)
            => SerializeClass(token) + SerializeModifiers(token as Element);

        private static string SerializeClass(Token token)
        {
            switch (token)
            {
                case Ellipsis _:
                case Terminator _: return ".";
                case Separator _: return ",";
                case Article _: return "T";
                case Noun _: return "N";
                case Pronoun _: return "R";
                case Adjective _: return "A";
                case Verb _: return "V";
                case Quantifier _:
                case Number _: return "Q";
                case Abbreviation _:
                case Unclassified _: return "U";
                default: throw new NotImplementedException();
            }
        }

        private static string SerializeModifiers(Element element)
            => element == null 
                ? "" 
                : new string(SerializeModifiers(element.Modifiers).ToArray());

        private static IEnumerable<char> SerializeModifiers(Modifier modifiers)
        {
            if (modifiers.HasFlag(Modifier.Definite))
                yield return 'd';
            if (modifiers.HasFlag(Modifier.Plural))
                yield return 'n';
            if (modifiers.HasFlag(Modifier.Possessive))
                yield return 'p';
            if (modifiers.HasFlag(Modifier.Qualified))
                yield return 'q';
            if (modifiers.HasFlag(Modifier.Present))
                yield return 'o';
        }

        private static int Code(Token token)
            => (ClassCode(token) << 8) + ModifierCode(token as Element);

        private static byte ClassCode(Token token)
        {
            switch (token)
            {
                case Ellipsis _:
                case Terminator _: return 1;
                case Separator _: return 2;
                case Article _: return 3;
                case Noun _: return 4;
                case Pronoun _: return 5;
                case Adjective _: return 6;
                case Verb _: return 7;
                case Quantifier _:
                case Number _: return 8;
                case Abbreviation _:
                case Unclassified _: return 255;
                default: throw new NotImplementedException();
            }
        }

        private static byte ModifierCode(Element element)
            => element == null ? (byte)0 : (byte)element.Modifiers;

        private static Token CreateToken(char primary, string modifierStr)
        {
            var modifiers = ParseModifiers(modifierStr);
            switch (primary)
            {
                case '.': return new Terminator(primary);
                case ',': return new Separator(primary);
                case 'T': return new Article { Modifiers = modifiers };
                case 'N': return new Noun { Modifiers = modifiers };
                case 'R': return new Pronoun { Modifiers = modifiers };
                case 'A': return new Adjective { Modifiers = modifiers };
                case 'V': return new Verb { Modifiers = modifiers };
                case 'Q': return new Number { Modifiers = modifiers };
                default: throw new NotImplementedException();
            }
        }

        private static Modifier ParseModifiers(string modifiers)
            => modifiers.Select(ToModifier).Aggregate(Modifier.None, (o, n) => o | n);

        private static Modifier ToModifier(char c)
        {
            switch (c)
            {
                case 'd': return Modifier.Definite;
                case 'n': return Modifier.Plural;
                case 'p': return Modifier.Possessive;
                case 'q': return Modifier.Qualified;
                case 'o': return Modifier.Present;
                default: throw new NotImplementedException();
            }
        }
    }
}