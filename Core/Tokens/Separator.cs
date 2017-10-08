namespace Lingua.Core.Tokens
{
    public class Separator : Punctuation
    {
        public const ushort Code = 2 << Encoder.ModifierBits;

        public Separator(char character) : base(character)
        {
        }
    }
}