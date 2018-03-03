namespace Lingua.Core.Tokens
{
    public class Aggregate : Token
    {
        public const ushort MinCode = 31 << Encoder.ModifierCount;

        public readonly ushort Code;
        public readonly ushort[] Pattern;

        public Aggregate(ushort code, ushort[] pattern)
        {
            Code = code;
            Pattern = pattern;
        }
    }
}