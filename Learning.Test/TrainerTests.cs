using NUnit.Framework;

namespace Lingua.Learning.Test
{
    [TestFixture]
    public class TrainerTests
    {
        [Test]
        public void TestAll()
        {
            var trainer = new Trainer();
            var res = GeneratePatterns(trainer);
            Assert.That(res.FailedCase?.Success ?? true, $"Failed on {res.SuccessCount + 1}th case");
        }

        [Test, Explicit("Saves generated patterns to file on success")]
        public void GeneratePatterns()
        {
            var trainer = new Trainer();
            var success = GeneratePatterns(trainer).Success;
            Assert.That(success, "Failed to generate and store patterns, failing testcase");
            trainer.SavePatterns();
        }

        [TestCase("I have been running", "jag har sprungit")]
        [TestCase("I will be running", "jag kommer att springa")]
        [TestCase("I will have been running", "jag kommer att ha sprungit")]
        [TestCase("It is my pen", "Det är min penna")]
        public void Test(string from, string expected)
        {
            var trainer = new Trainer();
            var testCase = new TestCase(from, expected);
            var res = trainer.RunTrainingSession(testCase);
            Assert.That(res.FailedCase?.Success ?? true, $"Failed on {res.SuccessCount + 1}th case");
        }

        private TestSessionResult GeneratePatterns(Trainer trainer)
        {
            var testCases = TestRunner.LoadTestCases();
            return trainer.RunTrainingSession(testCases);
        }
    }
}