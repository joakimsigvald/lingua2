namespace Lingua.Tokenization.Symbols
{
    public abstract class Symbol
    {
        public char Character { get; }
        protected Symbol(char c) => Character = c;
    }
}