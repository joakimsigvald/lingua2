using System.Linq;
using Lingua.Core;

namespace Lingua.Learning
{
    public class TranslationTarget
    {
        public Translation[] Translations { get; set; }
        public byte[] Order { get; set; }
        public bool IsInOrder => Order.Select((n, i) => n - i - 1).All(dif => dif == 0);
    }
}