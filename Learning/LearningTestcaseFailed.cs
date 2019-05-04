using System;
using System.Linq;

namespace Lingua.Learning
{
    public class LearningTestcaseFailed : Exception
    {
        public TestCaseResult Result { get; }

        public LearningTestcaseFailed(TestCaseResult result, string message)
            : base(string.Join("\n", CreateMessageLines(result).Prepend(message).ToArray()))
        {
            Result = result;
        }

        private static string[] CreateMessageLines(TestCaseResult result)
            => new[] {
                $"Failed testcase: '{result.TestCase.From}'",
                $"Expected: '{result.TestCase.Expected}'",
                $"Actual: '{result.Actual}'"
            };
    }
}