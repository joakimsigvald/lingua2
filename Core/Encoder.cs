using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core.Tokens;
using Lingua.Core.WordClasses;

namespace Lingua.Core
{
    public static class Encoder
    {
        public const byte ModifierBits = 11;
        public const ushort AnyMask = 0x07ff;

        public static ushort[] Encode(string serial)
            => Encode(Deserialize(serial)).ToArray();

        public static IEnumerable<ushort> Encode(IEnumerable<Token> tokens)
            => tokens.Select(Encode);

        public static string Serialize(IEnumerable<Token> tokens)
            => string.Join("", tokens.Select(Serialize));

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

        public static IEnumerable<Token> Decode(ushort[] code)
            => code.Select(Decode);

        public static bool Matches(ushort[] codes, ushort[] pattern)
            => codes.Length == pattern.Length
               && codes.Select((d, i) => Matches(d, pattern[i])).All(b => b);

        private static bool Matches(ushort code, ushort pattern)
            => code == pattern || (code | (ushort)Modifier.Any) == pattern;

        private static ushort Encode(Token token)
            => (ushort)((ClassCode(token) << ModifierBits) + ModifierCode(token as Element));

        private static Token Decode(ushort code)
        {
            var token = DecodeToken(code);
            if (token is Element element)
                element.Modifiers = DecodeModifiers(element, (ushort)(code & AnyMask));
            return token;
        }

        private static Token DecodeToken(int code)
        {
            switch (code >> ModifierBits)
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
                case 12: return new Conjunction();
                case 15: return new Greeting();
                case 31:
                    return new Unclassified();
                default: throw new NotImplementedException();
            }
        }

        private static Modifier DecodeModifiers(Element element, ushort code)
            => code == AnyMask
                ? Modifier.Any
                : DecodeAggregatedModifiers(element, code);

        private static Modifier DecodeAggregatedModifiers(Element element, ushort code)
            => Enumerable.Range(0, ModifierBits)
                    .Select(shift => element.DecodeModifier((ushort)(code & (1 << shift))))
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
                case Conjunction _: return "C";
                case Greeting _: return "G";
                case Abbreviation _:
                case Unclassified _: return "U";
                default: throw new NotImplementedException();
            }
        }

        private static Token CreateToken(char primary, string modifierStr)
        {
            var token = CreateUnmodifiedToken(primary);
            if (token is Element element)
                element.Modifiers = ParseModifiers(modifierStr);
            return token;
        }

        private static Token CreateUnmodifiedToken(char primary)
        {
            switch (primary)
            {
                case '^': return new Start();
                case '.': return new Terminator(primary);
                case ',': return new Separator(primary);
                case 'A': return new Adjective();
                case 'C': return new Conjunction();
                case 'G': return new Greeting();
                case 'I': return new InfinitiveMarker();
                case 'N': return new Noun();
                case 'P': return new Preposition();
                case 'Q': return new Number();
                case 'R': return new Pronoun();
                case 'T': return new Article();
                case 'V': return new Verb();
                case 'X': return new Auxiliary();
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
                case Conjunction _: return 12;
                case Greeting _: return 15;
                case Abbreviation _:
                case Unclassified _: return 31;
                default: throw new NotImplementedException();
            }
        }

        private static string SerializeModifiers(Element element)
            => Serialize(element, element?.Modifiers ?? Modifier.None);

        private static string Serialize(Element element, Modifier modifier)
            => modifier == Modifier.Any
                ? "*"
                : new string(SerializeModifiers(element, modifier).OrderBy(c => c).ToArray());

        private static IEnumerable<char> SerializeModifiers(Element element, Modifier modifiers)
        {
            if (modifiers.HasFlag(Modifier.Plural))
                yield return 'n';
            if (modifiers.HasFlag(Modifier.Definite))
                yield return 'd';
            if (modifiers.HasFlag(Modifier.Genitive))
                yield return 'g';
            if (modifiers.HasFlag(Modifier.Neuter) && (element is Adjective || element is Pronoun))
                yield return 't';
            if (modifiers.HasFlag(Modifier.Imperitive) && element is Verb)
                yield return 'i';
            if (modifiers.HasFlag(Modifier.Comparative) && element is Adjective)
                yield return 'c';
            if (modifiers.HasFlag(Modifier.Participle) && element is Verb)
                yield return 'l';
            if (modifiers.HasFlag(Modifier.Superlative) && element is Adjective)
                yield return 's';
            if (modifiers.HasFlag(Modifier.FirstPerson) && (element is Verb || element is Pronoun))
                yield return '1';
            if (modifiers.HasFlag(Modifier.Adverb) && element is Adjective)
                yield return 'a';
            if (modifiers.HasFlag(Modifier.SecondPerson) && (element is Verb || element is Pronoun))
                yield return '2';
            if (modifiers.HasFlag(Modifier.ThirdPerson))
                yield return '3';
            if (modifiers.HasFlag(Modifier.Past) && element is Verb)
                yield return 'p';
            if (modifiers.HasFlag(Modifier.Qualified) && element is Article)
                yield return 'q';
            if (modifiers.HasFlag(Modifier.Perfect) && element is Verb)
                yield return 'r';
            if (modifiers.HasFlag(Modifier.Object) && element is Pronoun)
                yield return 'o';
            if (modifiers.HasFlag(Modifier.Future) && element is Verb)
                yield return 'f';
            if (modifiers.HasFlag(Modifier.Possessive) && element is Pronoun)
                yield return 'm';
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
                case 'l': return Modifier.Participle;
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

        private static ushort ModifierCode(Element element)
            => (ushort) (element?.Modifiers ?? Modifier.None);
    }
}