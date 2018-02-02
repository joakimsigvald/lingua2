using System;

namespace Lingua.Learning
{
    public class InvalidExample : Exception
    {
        public InvalidExample(TestCase testCase, string message)
            : base($"{message}: {testCase}")
        {
        }
    }
}