namespace Lingua.Core.Tokens
{
    public class Terminator : Punctuation
    {
        public const ushort Code = 1 << Encoder.ModifierBits;

        public Terminator(char character) : base(character)
        {
        }
    }
}