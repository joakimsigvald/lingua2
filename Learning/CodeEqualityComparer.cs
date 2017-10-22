using System.Collections.Generic;
using System.Linq;

namespace Lingua.Testing
{
    public class CodeEqualityComparer : IEqualityComparer<ushort[]>
    {
        public static readonly CodeEqualityComparer Singleton = new CodeEqualityComparer();

        private static readonly ushort[] Empty = new ushort[0];

        public bool Equals(ushort[] x, ushort[] y)
            => x?.SequenceEqual(y ?? Empty) ?? false;

        public int GetHashCode(ushort[] code) => code[0];
    }
}