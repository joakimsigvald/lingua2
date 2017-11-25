using System.Linq;
using Lingua.Core;

namespace Lingua.Learning
{
    public class TranslationTarget
    {
        public Translation[] Translations { get; set; }
        public byte[] Order { get; set; }
        public bool IsRearranged => Order.Select((n, i) => (n, i)).Any(ni => ni.Item1 != ni.Item2);
    }
}