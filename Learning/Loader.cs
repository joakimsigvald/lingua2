using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lingua.Learning
{
    using Core;

    public static class Loader
    {
        public static Dictionary<string, Dictionary<string, string>> LoadTestSuites()
        {
            var lines = LoaderBase.ReadFile("TestCases.txt")
                .Where(line => !string.IsNullOrWhiteSpace(line));
            var testSuites = new Dictionary<string, Dictionary<string, string>>();
            string caption = null;
            var testSuite = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                if (IsCaption(line))
                {
                    if (!string.IsNullOrEmpty(caption))
                    {
                        testSuites[caption] = testSuite;
                        testSuite = new Dictionary<string, string>();
                    }
                    caption = ExtractCaption(line);
                }
                else
                {
                    (var from, var to) = ExtractTestCase(line);
                    testSuite[from] = to;
                }
            }
            if (!string.IsNullOrEmpty(caption))
                testSuites[caption] = testSuite;
            return testSuites;
        }

        private static (string from, string to) ExtractTestCase(string line)
        {
            var pair = Regex.Split(line, "\\\",\\s\\\"");
            return (pair[0].Trim('\"'), pair[1].Trim('\"'));
        }

        private static string ExtractCaption(string line)
            => line.Substring(2);

        private static bool IsCaption(string line)
            => line.StartsWith("//") && !line.StartsWith("///");
    }
}