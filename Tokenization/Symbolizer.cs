using System;
using System.Linq;
using Lingua.Core.WordClasses;

namespace Lingua.Tokenization
{
    using Core;
    using Core.Tokens;
    using Symbols;

    public interface ISymbolizer
    {
        Symbol[] Symbolize(string text);
        bool TryGetNextToken(Symbol symbol, Symbol prev, Symbol next, out Token nextToken);
        Token GetNextToken(Symbol current, Symbol prev, Symbol next);
    }

    public class Symbolizer : ISymbolizer
    {
        public Symbol[] Symbolize(string text)
            => text.Trim()
                .Select(GetSymbol)
                .Append(NoSymbol.Singleton)
                .ToArray();


        public bool TryGetNextToken(Symbol symbol, Symbol prev, Symbol next, out Token nextToken)
        {
            nextToken = GetNextToken(symbol, prev, next);
            return nextToken != null;
        }

        public Token GetNextToken(Symbol current, Symbol prev, Symbol next)
        {
            if (current.GetType() == prev.GetType())
                return null;
            switch (current)
            {
                case Letter _:
                    return prev is Dot || prev is Dash ? null : new Unclassified();
                case Digit _:
                    return prev is Dot || prev is Dash ? null : new Number();
                case Space _:
                    return new Divider();
                case Dot dot:
                    return prev is Digit && next is Digit
                        ? null
                        : prev is Letter && next is Letter
                            ? null
                            : next is Dot
                                ? (Token) new Ellipsis()
                                : new Terminator(dot.Character);
                case Mark mark: return new Terminator(mark.Character);
                case Comma comma: return new Separator(comma.Character);
                case Dash _:
                    if (next is Digit) return new Number();
                    if (prev is Letter) return null;
                    break;
                case LeftBracket _:
                    if (next is LeftBracket) return new StartGeneric();
                    break;
                case RightBracket _:
                    if (next is RightBracket) return new EndGeneric();
                    break;
            }
            throw new NotImplementedException();
        }

        private static readonly char[] Marks = "!?:".ToArray();
        private static readonly char[] Commas = ",;".ToArray();

        private static Symbol GetSymbol(char c)
        {
            if (char.IsWhiteSpace(c))
                return new Space(c);
            if (char.IsLetter(c))
                return new Letter(c);
            if (char.IsDigit(c))
                return new Digit(c);
            if (Marks.Contains(c))
                return new Mark(c);
            if (Commas.Contains(c))
                return new Comma(c);
            switch (c)
            {
                case '\'': return new Letter(c);
                case '.': return new Dot(c);
                case '-': return new Dash(c);
                case '[': return new LeftBracket(c);
                case ']': return new RightBracket(c);
            }
            throw new NotImplementedException();
        }
    }
}