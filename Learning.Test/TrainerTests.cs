using NUnit.Framework;

namespace Lingua.Learning.Test
{
    [TestFixture]
    public class TrainerTests
    {
        [Test, Category("Longrunning")]
        public void TestAll()
        {
            var trainer = new Trainer();
            var res = GeneratePatterns(trainer);
            var reporter = new ConsoleReporter();
            reporter.Report(res);
            Assert.That(res.Success, $"Failed on {res.SuccessCount + 1}th case");
        }

        [Test, Category("Longrunning"), Explicit("Saves generated patterns to file on success")]
        public void GeneratePatterns()
        {
            var trainer = new Trainer();
            var success = GeneratePatterns(trainer).Success;
            Assert.That(success, "Failed to generate and store patterns, failing testcase");
            trainer.SavePatterns();
        }

        [TestCase("I am here", "jag är här")]
        [TestCase("I want to go to the theater", "jag vill gå till teatern")]
        [TestCase("A ball i.e..", "En boll dvs...")]
        [TestCase("The rat made a nest and slept in it.", "Råttan gjorde ett bo och sov i det.")]
        [TestCase("I have been running", "jag har sprungit")]
        [TestCase("I will be running", "jag kommer att springa")]
        [TestCase("I will have been running", "jag kommer att ha sprungit")]
        [TestCase("It is my pen", "Det är min penna")]
        [TestCase("The cat caught a rat and ate it", "Katten fångade en råtta och åt den")]
        [TestCase("The cat caught a rat", "Katten fångade en råtta")]
        [TestCase("He runs. I run.", "Han springer. Jag springer.")]
        public void Test(string from, string expected)
        {
            var trainer = new Trainer();
            var testCase = new TestCase(from, expected);
            var res = trainer.RunTrainingSession(testCase);
            Assert.That(res.FailedCase?.Success ?? true, $"Failed on {res.SuccessCount + 1}th case");
        }

        private static TestSessionResult GeneratePatterns(Trainer trainer)
        {
            var testCases = TestRunner.LoadTestCases();
            return trainer.RunTrainingSession(testCases);
        }
    }
}