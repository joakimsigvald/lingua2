using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core
{
    using Extensions;
    using Tokens;
    using WordClasses;

    public static class Encoder
    {
        public const ushort Wildcard = 1 << 10;
        public const byte ModifierCount = 11;
        private const ushort ClassMask = 0xf800;
        public const ushort ModifiersMask = 0x07ff;

        public static ushort[] Encode(string serial)
            => Encode(Deserialize(serial)).ToArray();

        public static ushort[] Encode(IEnumerable<Translation> translations)
            => Encode(translations.Select(t => t.From));

        public static ushort[] Encode(IEnumerable<Token> tokens)
            => tokens.Select(Encode).ToArray();

        public static ushort Encode(Token token)
            => (ushort)(ClassCode(token) + ModifierCode(token as Element));

        public static string Serialize(IEnumerable<ushort> code)
            => string.Join("", Decode(code).Select(Serialize));

        public static string Serialize(IEnumerable<Token> tokens)
            => string.Join("", tokens.Select(Serialize));

        public static string Serialize(Token token)
            => SerializeClass(token) + SerializeModifiers(token as Element);

        public static IEnumerable<Token> Deserialize(string serial)
        {
            if (string.IsNullOrEmpty(serial))
                yield break;

            var modifiers = "";
            var primary = serial[0];
            foreach (var c in serial.Skip(1))
            {
                if (IsModifier(c))
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

        public static Modifier ParseModifiers(string modifiers)
            => (modifiers ?? "").Select(ToModifier).Aggregate(Modifier.None, (o, n) => o | n);

        public static IEnumerable<Token> Decode(IEnumerable<ushort> code)
            => code.Select(Decode);

        public static bool Matches(ushort[] codes, ushort[] pattern)
            => codes.Length == pattern.Length
               && codes.Select((d, i) => Matches(d, pattern[i])).All(b => b);

        public static bool Matches(ushort code, ushort pattern)
            => MatchesExact(code, pattern) || MatchesWithWildcard(code, pattern);

        private static bool MatchesExact(ushort code, ushort pattern)
            => code == pattern;

        private static bool MatchesWithWildcard(ushort code, ushort pattern)
            => (pattern ^ Wildcard) == (pattern & code);

        private static Token Decode(ushort code)
        {
            var token = DecodeToken(code);
            if (token is Element element)
                element.Modifiers = DecodeModifiers(element, (ushort)(code & ModifiersMask));
            return token;
        }

        private static Token DecodeToken(ushort code)
        {
            switch (code & ClassMask)
            {
                case Start.Code: return Start.Singleton;
                case Terminator.Code: return new Terminator('.');
                case Separator.Code: return new Separator(',');
                case Number.Code: return new Number();
                case Noun.Code: return new Noun();
                case Article.Code: return new Article();
                case Preposition.Code: return new Preposition();
                case Pronoun.Code: return new Pronoun();
                case Adjective.Code: return new Adjective();
                case Auxiliary.Code: return new Auxiliary();
                case Verb.Code: return new Verb();
                case InfinitiveMarker.Code: return new InfinitiveMarker();
                case Conjunction.Code: return new Conjunction();
                case Greeting.Code: return new Greeting();
                case Unclassified.Code: return new Unclassified();
                default: throw new NotImplementedException();
            }
        }

        private static ushort ClassCode(Token token)
        {
            switch (token)
            {
                case Start _: return Start.Code;
                case Ellipsis _:
                case Terminator _: return Terminator.Code;
                case Separator _: return Separator.Code;
                case Quantifier _:
                case Number _: return Number.Code;
                case Noun _: return Noun.Code;
                case Article _: return Article.Code;
                case Preposition _: return Preposition.Code;
                case Pronoun _: return Pronoun.Code;
                case Adjective _: return Adjective.Code;
                case Auxiliary _: return Auxiliary.Code;
                case Verb _: return Verb.Code;
                case InfinitiveMarker _: return InfinitiveMarker.Code;
                case Conjunction _: return Conjunction.Code;
                case Greeting _: return Greeting.Code;
                case Abbreviation _:
                case Unclassified _: return Unclassified.Code;
                default: throw new NotImplementedException();
            }
        }

        private static Modifier DecodeModifiers(Element element, ushort code)
            => ModifierBits
                .Select(modifier => element.DecodeModifier((ushort)(code & modifier)))
                .Aggregate(Modifier.None, (a, b) => a | b);

        private static readonly ushort[] ModifierBits
            = Enumerable.Range(0, ModifierCount).Select(shift => (ushort)(1 << shift)).ToArray();

        private static bool IsModifier(char c)
            => char.IsLower(c) || char.IsDigit(c) || c == '*';

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
                case '^': return Start.Singleton;
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

        private static string SerializeModifiers(Element element)
            => Serialize(element, element?.Modifiers ?? Modifier.None);

        private static string Serialize(Element element, Modifier modifier)
            => new string(SerializeModifiers(element, modifier).ToArray());

        private static IEnumerable<char> SerializeModifiers(Element element, Modifier modifiers)
        {
            if (modifiers.HasFlag(Modifier.Plural))
                yield return 'n';
            if (modifiers.HasFlag(Modifier.Definite))
                yield return 'd';
            if (modifiers.HasFlag(Modifier.Genitive) && !(element is Verb))
                yield return 'g';
            if (modifiers.HasFlag(Modifier.Neuter) && !(element is Verb))
                yield return 't';
            if (modifiers.HasFlag(Modifier.Imperitive) && element is Verb)
                yield return 'i';
            if (modifiers.HasFlag(Modifier.Participle) && element is Verb)
                yield return 'l';
            if (modifiers.HasFlag(Modifier.Comparative) && element is Adjective)
                yield return 'c';
            if (modifiers.HasFlag(Modifier.Superlative) && element is Adjective)
                yield return 's';
            if (modifiers.HasFlag(Modifier.Adverb) && element is Adjective)
                yield return 'a';
            if (modifiers.HasFlag(Modifier.FirstPerson) && !(element is Adjective))
                yield return '1';
            if (modifiers.HasFlag(Modifier.SecondPerson) && !(element is Adjective))
                yield return '2';
            if (modifiers.HasFlag(Modifier.ThirdPerson) && !(element is Adjective))
                yield return '3';
            if (modifiers.HasFlag(Modifier.Past) && element is Verb)
                yield return 'p';
            if (modifiers.HasFlag(Modifier.Perfect) && element is Verb)
                yield return 'r';
            if (modifiers.HasFlag(Modifier.Future) && element is Verb)
                yield return 'f';
            if (modifiers.HasFlag(Modifier.Object) && element is Pronoun)
                yield return 'o';
            if (modifiers.HasFlag(Modifier.Possessive) && element is Pronoun)
                yield return 'm';
            if (modifiers.HasFlag(Modifier.Qualified) && element is Article)
                yield return 'q';
            if (modifiers.HasFlag(Modifier.Wildcard))
                yield return '*';
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
                case '*': return Modifier.Wildcard;
                default: throw new NotImplementedException();
            }
        }

        private static ushort ModifierCode(Element element)
            => (ushort) (element?.Modifiers ?? Modifier.None);

        public static IEnumerable<ushort> Generalize(ushort code)
            => Reduce(code).Distinct().Append(code).Select(rc => (ushort)(rc | Wildcard));

        private static IEnumerable<ushort> Reduce(ushort code)
            => ModifierBits
                .Select(modifier => (ushort) (modifier ^ code))
                .Where(altered => altered < code)
                .SelectMany(reduced => Reduce(reduced).Append(reduced));
    }
}