using System.Linq;

namespace Lingua.Learning
{
    public class TestSessionResult
    {
        public readonly TestCaseResult[] Results;
        public readonly TestCaseResult FailedCase;
        public readonly int SuccessCount;

        public TestSessionResult(params TestCaseResult[] results)
        {
            Results = results;
            SuccessCount = Results.Any() ? Results.TakeWhile(r => r.Success).Count() : -1;
            FailedCase = Results.Skip(SuccessCount).FirstOrDefault();
        }

        private int Deficit => FailedCase?.Deficit ?? 0;

        public bool Success => Results.Any() && FailedCase == null;

        public static bool operator <(TestSessionResult tsr1, TestSessionResult tsr2)
            => tsr1.SuccessCount < tsr2.SuccessCount
            || tsr1.SuccessCount == tsr2.SuccessCount && tsr1.Deficit > tsr2.Deficit;

        public static bool operator >(TestSessionResult tsr1, TestSessionResult tsr2)
            => tsr2 < tsr1;
    }
}