using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;

namespace Lingua.Core
{
    public static class Encoder
    {
        public static IEnumerable<int> Encode(IEnumerable<Token> tokens)
            => tokens.Where(t => !(t is Divider)).Select(Encode);

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
                if (IsWordClass(c))
                {
                    yield return CreateToken(primary, modifiers);
                    primary = c;
                    modifiers = "";
                }
                else
                    modifiers += c;
            }
            yield return CreateToken(primary, modifiers);
        }

        public static Modifier ParseModifiers(string modifiers)
            => (modifiers ?? "").Select(ToModifier).Aggregate(Modifier.None, (o, n) => o | n);

        public static IEnumerable<Token> Decode(int[] code)
            => code.Select(Decode);

        public static bool Matches(int[] codes, int[] pattern)
            => codes.Length == pattern.Length
               && codes.Select((d, i) => Matches(d, pattern[i])).All(b => b);

        private static bool Matches(int code, int pattern)
            => code == pattern || (code | (int)Modifier.Any) == pattern;

        private static int Encode(Token token)
            => (ClassCode(token) << 16) + ModifierCode(token as Element);

        private static Token Decode(int code)
        {
            var token = DecodeToken(code);
            if (token is Element element)
                element.Modifiers = DecodeModifiers(code);
            return token;
        }

        private static Token DecodeToken(int code)
        {
            switch (code >> 16)
            {
                case 0: return new Start();
                case 1: return new Terminator('.');
                case 2: return new Separator(',');
                case 3: return new Number();
                case 4: return new Noun();
                case 5: return new Article();
                case 6: return new Preposition();
                case 7: return new Pronoun();
                case 8: return new Adjective();
                case 9: return new Auxiliary();
                case 10: return new Verb();
                case 11: return new InfinitiveMarker();
                case 255:
                    return new Unclassified();
                default: throw new NotImplementedException();
            }
        }

        private static Modifier DecodeModifiers(int code) => Enumerable.Range(0, 16)
            .Select(shift => (Modifier) (code & (1 << shift)))
            .Aggregate(Modifier.None, (a, b) => a | b);

        private static bool IsWordClass(char c)
            => char.IsUpper(c);

        private static string Serialize(Token token)
            => SerializeClass(token) + SerializeModifiers(token as Element);

        private static string SerializeClass(Token token)
        {
            switch (token)
            {
                case Start _: return "^";
                case Ellipsis _:
                case Terminator _: return ".";
                case Separator _: return ",";
                case Quantifier _:
                case Number _: return "Q";
                case Noun _: return "N";
                case Article _: return "T";
                case Preposition _: return "P";
                case Pronoun _: return "R";
                case Adjective _: return "A";
                case Auxiliary _: return "X";
                case Verb _: return "V";
                case InfinitiveMarker _: return "I";
                case Abbreviation _:
                case Unclassified _: return "U";
                default: throw new NotImplementedException();
            }
        }

        private static Token CreateToken(char primary, string modifierStr)
        {
            var modifiers = ParseModifiers(modifierStr);
            switch (primary)
            {
                case '^': return new Start();
                case '.': return new Terminator(primary);
                case ',': return new Separator(primary);
                case 'Q': return new Number { Modifiers = modifiers };
                case 'N': return new Noun {Modifiers = modifiers};
                case 'T': return new Article { Modifiers = modifiers };
                case 'P': return new Preposition { Modifiers = modifiers };
                case 'R': return new Pronoun { Modifiers = modifiers };
                case 'A': return new Adjective {Modifiers = modifiers};
                case 'X': return new Auxiliary { Modifiers = modifiers };
                case 'V': return new Verb { Modifiers = modifiers };
                case 'I': return new InfinitiveMarker { Modifiers = modifiers };
                default: throw new NotImplementedException();
            }
        }

        private static byte ClassCode(Token token)
        {
            switch (token)
            {
                case Start _: return 0;
                case Ellipsis _:
                case Terminator _: return 1;
                case Separator _: return 2;
                case Quantifier _:
                case Number _: return 3;
                case Noun _: return 4;
                case Article _: return 5;
                case Preposition _: return 6;
                case Pronoun _: return 7;
                case Adjective _: return 8;
                case Auxiliary _: return 9;
                case Verb _: return 10;
                case InfinitiveMarker _: return 11;
                case Abbreviation _:
                case Unclassified _: return 255;
                default: throw new NotImplementedException();
            }
        }

        private static string SerializeModifiers(Element element)
            => Serialize(element?.Modifiers ?? Modifier.None);

        private static string Serialize(Modifier modifier)
            => modifier == Modifier.Any
                ? "*"
                : new string(SerializeModifiers(modifier).ToArray());

        private static IEnumerable<char> SerializeModifiers(Modifier modifiers)
        {
            if (TrySerializePersonModifiers(modifiers, out char c))
                yield return c;
            if (modifiers.HasFlag(Modifier.Adverb))
                yield return 'a';
            if (modifiers.HasFlag(Modifier.Comparative))
                yield return 'c';
            if (modifiers.HasFlag(Modifier.Definite))
                yield return 'd';
            if (modifiers.HasFlag(Modifier.Future))
                yield return 'f';
            if (modifiers.HasFlag(Modifier.Genitive))
                yield return 'g';
            if (modifiers.HasFlag(Modifier.Imperitive))
                yield return 'i';
            if (modifiers.HasFlag(Modifier.Possessive))
                yield return 'm';
            if (modifiers.HasFlag(Modifier.Plural))
                yield return 'n';
            if (modifiers.HasFlag(Modifier.Object))
                yield return 'o';
            if (modifiers.HasFlag(Modifier.Past))
                yield return 'p';
            if (modifiers.HasFlag(Modifier.Qualified))
                yield return 'q';
            if (modifiers.HasFlag(Modifier.Perfect))
                yield return 'r';
            if (modifiers.HasFlag(Modifier.Superlative))
                yield return 's';
            if (modifiers.HasFlag(Modifier.Neuter))
                yield return 't';
        }

        private static bool TrySerializePersonModifiers(Modifier modifiers, out char c)
        {
            var res = SerializePersonModifiers(modifiers);
            c = res ?? (char) 0;
            return res.HasValue;
        }

        private static char? SerializePersonModifiers(Modifier modifiers)
        {
            if (modifiers.HasFlag(Modifier.ThirdPerson))
                return '3';
            if (modifiers.HasFlag(Modifier.SecondPerson))
                return '2';
            if (modifiers.HasFlag(Modifier.FirstPerson))
                return '1';
            return null;
        }

        private static Modifier ToModifier(char c)
        {
            switch (c)
            {
                case '1': return Modifier.FirstPerson;
                case '2': return Modifier.SecondPerson;
                case '3': return Modifier.ThirdPerson;
                case 'a': return Modifier.Adverb;
                case 'c': return Modifier.Comparative;
                case 'd': return Modifier.Definite;
                case 'f': return Modifier.Future;
                case 'g': return Modifier.Genitive;
                case 'i': return Modifier.Imperitive;
                case 'm': return Modifier.Possessive;
                case 'n': return Modifier.Plural;
                case 'o': return Modifier.Object;
                case 'p': return Modifier.Past;
                case 'q': return Modifier.Qualified;
                case 'r': return Modifier.Perfect;
                case 's': return Modifier.Superlative;
                case 't': return Modifier.Neuter;
                case '*': return Modifier.Any;
                default: throw new NotImplementedException();
            }
        }

        private static int ModifierCode(Element element)
            => (int) (element?.Modifiers ?? Modifier.None);
    }
}