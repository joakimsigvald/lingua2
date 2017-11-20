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
            var testCases = TestRunner.LoadTestCases();
            var res = trainer.RunTrainingSession(testCases);
            Assert.That(res.FailedCase?.Success ?? true, $"Failed on {res.SuccessCount + 1}th case");
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
    }
}