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
            Assert.That(res.Item1?.Success ?? true, $"Failed on {res.Item2}th case");
        }

        [TestCase("I will be running", "jag kommer att springa")]
        public void Test(string from, string expected)
        {
            var trainer = new Trainer();
            var testCase = new TestCase(from, expected);
            var res = trainer.RunTrainingSession(testCase);
            Assert.That(res.Item1?.Success ?? true, $"Failed on {res.Item2}th case");
        }
    }
}