namespace Lingua.Core.Tokens
{
    public abstract class Punctuation : Token
    {
        private readonly char _character;
        protected Punctuation(char character) => _character = character;
        public override string Value => $"{_character} ";
    }
}