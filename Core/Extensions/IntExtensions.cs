namespace Lingua.Core.Extensions
{
    public static class IntExtensions
    {
        public static byte CountBits(this int number)
        {
            var count = 0;
            while (number != 0)
            {
                count += number & 1;
                number = number >> 1;
            }
            return (byte)count;
        }
    }
}
