namespace Lingua.Tokenization.Symbols
{
    public class NoSymbol : Symbol
    {
        public static readonly NoSymbol Singleton = new NoSymbol();

        private NoSymbol() : base(char.MinValue)
        {
        }
    }
}