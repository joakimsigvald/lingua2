namespace Lingua.Core.Tokens
{
    public class Terminator : Punctuation
    {
        public const ushort Code = 1 << Encoder.ModifierCount;

        public Terminator(char character) : base(character)
        {
        }
    }
}