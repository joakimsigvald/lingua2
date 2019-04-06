using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core
{
    using Tokens;
    using WordClasses;

    public static class Encoder
    {
        public const ushort Wildcard = 1 << 10;
        public const byte ModifierCount = 11;
        private const ushort ClassMask = 0xf800;
        private const ushort ModifiersMask = 0x07ff;
        public const ushort ProperModifiersMask = 0x03ff;

        public static Code Encode(string serial)
            => new Code(Encode(Deserialize(serial)).Reverse());

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

        public static object Encode(object reversedCode)
        {
            throw new NotImplementedException();
        }

        public static Modifier ParseModifiers(string modifiers)
            => (modifiers ?? "").Select(ToModifier).Aggregate(Modifier.None, (o, n) => o | n);

        public static IEnumerable<Token> Decode(IEnumerable<ushort> code)
            => code.Select(Decode);

        public static bool Matches(Code code, Code pattern)
            => Matches(code.ReversedCode, pattern.ReversedCode);

        public static bool Matches(ushort[] codes, ushort[] pattern)
            => codes.Length == pattern.Length
               && codes.Select((d, i) => Matches(d, pattern[i])).All(b => b);

        public static bool Matches(ushort code, ushort pattern)
            => code == pattern 
            || pattern == AnyToken.Code 
            || (pattern ^ Wildcard) == ((pattern | ClassMask) & code);

        public static ushort Recode<TElement>(ushort code)
            where TElement : Element, new()
        {
            var token = DecodeToken(code);
            if (!(token is Element element))
                throw new InvalidOperationException("Can only recode elements to other elements");
            var newElement = new TElement
            {
                Modifiers = DecodeModifiers(element, (ushort)(code & ModifiersMask))
            };
            return Encode(newElement);
        }

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
                case NounPhrase.Code: return new NounPhrase();
                case Article.Code: return new Article();
                case Preposition.Code: return new Preposition();
                case Pronoun.Code: return new Pronoun();
                case Adjective.Code: return new Adjective();
                case Auxiliary.Code: return new Auxiliary();
                case Verb.Code: return new Verb();
                case InfinitiveMarker.Code: return new InfinitiveMarker();
                case Conjunction.Code: return new Conjunction();
                case Greeting.Code: return new Greeting();
                case AdverbQuestion.Code: return new AdverbQuestion();
                case AdverbPositioning.Code: return new AdverbPositioning();
                case AdverbQualifying.Code: return new AdverbQualifying();
                case Name.Code: return new Name();
                case Unclassified.Code: return new Unclassified();
                case AnyToken.Code: return new AnyToken();
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
                case NounPhrase _: return NounPhrase.Code;
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
                case AdverbQuestion _: return AdverbQuestion.Code;
                case AdverbPositioning _: return AdverbPositioning.Code;
                case AdverbQualifying _: return AdverbQualifying.Code;
                case Name _: return Name.Code;
                case Abbreviation _:
                case Unclassified _: return Unclassified.Code;
                case AnyToken _: return AnyToken.Code;
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
                case AdverbQuestion _: return "?";
                case AdverbPositioning _: return "!";
                case Adjective _: return "A";
                case AdverbQualifying _: return "B";
                case Conjunction _: return "C";
                case Greeting _: return "G";
                case InfinitiveMarker _: return "I";
                case NounPhrase _: return "D";
                case Noun _: return "N";
                case Name _: return "E";
                case Preposition _: return "P";
                case Quantifier _:
                case Number _: return "Q";
                case Pronoun _: return "R";
                case Article _: return "T";
                case Abbreviation _:
                case Unclassified _: return "U";
                case Auxiliary _: return "X";
                case Verb _: return "V";
                case AnyToken _ : return "_";
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
                case '?': return new AdverbQuestion();
                case '!': return new AdverbPositioning();
                case 'A': return new Adjective();
                case 'B': return new AdverbQualifying();
                case 'C': return new Conjunction();
                case 'E': return new Name();
                case 'G': return new Greeting();
                case 'I': return new InfinitiveMarker();
                case 'N': return new Noun();
                case 'D': return new NounPhrase();
                case 'P': return new Preposition();
                case 'Q': return new Number();
                case 'R': return new Pronoun();
                case 'T': return new Article();
                case 'V': return new Verb();
                case 'X': return new Auxiliary();
                case '_': return new AnyToken();
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
            if (modifiers.HasFlag(Modifier.Continuous) && element is Verb)
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
                case 'l': return Modifier.Continuous;
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
            => GeneralizeModifiers(code)
                .Prepend(AnyToken.Code);

        public static IEnumerable<ushort> GeneralizeModifiers(ushort code)
            => Reduce(code)
                .Distinct()
                .Append(code)
                .Select(rc => (ushort)(rc | Wildcard));

        private static IEnumerable<ushort> Reduce(ushort code)
            => ModifierBits
                .Select(modifier => (ushort) (modifier ^ code))
                .Where(altered => altered < code)
                .SelectMany(reduced => Reduce(reduced).Append(reduced));

        public static ushort GetClassCode(ushort code)
            => (ushort)(code >> ModifierCount);

        public static bool IsElement(ushort code)
            => code >= Number.Code;
    }
}