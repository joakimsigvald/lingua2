using System.Collections.Generic;
using System.Linq;

namespace Lingua.TestUtils
{
    public static class Assert
    {
        public static void Equivalent<T>(ISet<T> expected, ICollection<T> actual)
        {
            var missing = expected.Except(expected).ToArray();
            var extra = actual.Except(actual).ToArray();
            Xunit.Assert.True(expected.Count == actual.Count && !missing.Any() && !extra.Any()
                , ShowDifference(missing, extra));
        }

        private static string ShowDifference<T>(T[] missing, T[] extra)
            => $"Missing: {Join(missing)}; extra: {Join(extra)}";

        private static string Join<T>(IEnumerable<T> items) => string.Join(", ", items);
    }
}