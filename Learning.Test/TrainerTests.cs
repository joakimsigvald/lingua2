using Xunit;

namespace Lingua.Learning.Test
{
    public class TrainerTests
    {
        [Fact]
        [Trait("Category", "Longrunning")]
        public void TestAll()
        {
            var trainer = new Trainer();
            var res = GeneratePatterns(trainer);
            var reporter = new ConsoleReporter();
            reporter.Report(res);
            Assert.True(res.Success, $"Failed on {res.SuccessCount + 1}th case");
        }

        [Trait("Category", "Longrunning")]
        [Fact(Skip="Saves generated patterns to file on success")]
        public void GenerateAndStorePatterns()
        {
            var trainer = new Trainer();
            var success = GeneratePatterns(trainer).Success;
            Assert.True(success, "Failed to generate and store patterns, failing testcase");
            trainer.SavePatterns();
        }

        [Theory]
        [InlineData("I will run", "jag ska springa")]
        [InlineData("I have run", "jag har sprungit")]
        [InlineData("Are you painting?", "Målar du?")]
        [InlineData("Today I have been to the concert hall.", "Idag har jag varit till konserthallen.")]
        [InlineData("I have been to the concert hall.", "Jag har varit till konserthallen.")]
        [InlineData("have been to the concert hall.", "har varit till konserthallen.")]
        [InlineData("been to the concert hall.", "varit till konserthallen.")]
        [InlineData("to the concert hall.", "till konserthallen.")]
        [InlineData("the concert hall.", "konserthallen.")]
        [InlineData("I am here", "jag är här")]
        [InlineData("I want to go to the theater", "jag vill gå till teatern")]
        [InlineData("A ball i.e..", "En boll dvs...")]
        [InlineData("The rat made a nest and slept in it.", "Råttan gjorde ett bo och sov i det.")]
        [InlineData("I have been running", "jag har sprungit")]
        [InlineData("I will be running", "jag kommer att springa")]
        [InlineData("I will have been running", "jag kommer att ha sprungit")]
        [InlineData("It is my pen", "Det är min penna")]
        [InlineData("The cat caught a rat and ate it", "Katten fångade en råtta och åt den")]
        [InlineData("The cat caught a rat", "Katten fångade en råtta")]
        [InlineData("He runs. I run.", "Han springer. Jag springer.")]
        [InlineData("The cat found an apple and ate it", "Katten hittade ett äpple och åt det")]
        [InlineData("the assistant university director", "biträdande universitetsdirektören")]
        [InlineData("the Assistant University Director", "biträdande universitetsdirektören")]
        [InlineData("Decisions on further handling of cases are taken by the Assistant University Director.", "Beslut om fortsatt handläggning fattas av biträdande universitetsdirektören.")]
        [InlineData("Albrecht's Gold", "Albrekts Guld")]
        public void Test(string from, string expected)
        {
            var trainer = new Trainer();
            var testCase = new TestCase(from, expected);
            var res = trainer.RunTrainingSession(testCase);
            Assert.True(res.FailedCase?.Success ?? true, $"Failed on {res.SuccessCount + 1}th case");
        }

        private static TestSessionResult GeneratePatterns(Trainer trainer)
        {
            var testCases = TestRunner.LoadTestCases();
            return trainer.RunTrainingSession(testCases);
        }
    }
}