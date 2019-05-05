using Lingua.Grammar;
using System.Linq;
using Xunit;

namespace Lingua.Learning.Test
{
    public class TrainerTests
    {
        [Trait("Category", "Longrunning")]
        [Theory]
        [InlineData(true, true, true)]
        public void TestAll(bool loadCurrentEvaluator, bool saveOnSuccess, bool overwrite)
        {
            var evaluator = loadCurrentEvaluator
                ? Evaluator.Load()
                : Evaluator.Create();
            var trainer = new Trainer(evaluator);
            var res = GeneratePatterns(trainer);
            var reporter = new ConsoleReporter();
            reporter.Report(res);
            Assert.True(res.Success, $"Failed on {res.SuccessCount + 1}th case");
            if (saveOnSuccess)
                trainer.SavePatterns(overwrite);
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
        [InlineData("Camilla currently works as an assistant on the product department at Albrecht's Gold.", "Camilla arbetar idag som assistent på produktavdelningen på Albrekts Guld.")]
        [InlineData("Despite this his assistant, or secretary, voted here with Mr Becali's MEP card.", "Trots detta röstade hans assistent, eller sekreterare, här med herr Becalis ledamotskort.")]
        [InlineData("When he was my assistant, he was a very good policeman... with a bright future.", "När han var min assistent var han en bra polis... med en ljus framtid.")]
        [InlineData("As regards the delegation, the delegate has been appointed, as has the assistant delegate.", "Vad gäller delegationen är representanten utsedd, liksom den biträdande representanten.")]
        public void Test(string from, string expected)
        {
            var trainer = new Trainer();
            var testCase = new TestCase(from, expected);
            var res = trainer.RunTrainingSession(testCase);
            Assert.True(res.FailedCase?.Success ?? true, $"Failed on {res.SuccessCount + 1}th case");
        }

        [Theory]
        [InlineData(
            "a red ball=>en röd boll",
            "This yellow cheese is old=>Den här gula osten är gammal")]
        [InlineData(
            "a red ball=>en röd boll",
            "The red ball=>Den röda bollen",
            "I have run=>jag har sprungit",
            "Decisions on further handling of cases are taken by the Assistant University Director.=>Beslut om fortsatt handläggning fattas av biträdande universitetsdirektören.",
            "As regards the delegation, the delegate has been appointed, as has the assistant delegate.=>Vad gäller delegationen är representanten utsedd, liksom den biträdande representanten.")]
        public void TestMany(params string[] examples)
        {
            var testCases = examples
                .Select(ex => ex.Split("=>"))
                .Select(p => new TestCase(p[0], p[1]))
                .ToArray();
            var trainer = new Trainer();
            var res = trainer.RunTrainingSession(testCases);
            Assert.True(res.FailedCase?.Success ?? true, $"Failed on {res.SuccessCount + 1}th case");
        }

        private static TestSessionResult GeneratePatterns(Trainer trainer)
        {
            var testCases = TestRunner.LoadTestCases();
            return trainer.RunTrainingSession(testCases);
        }
    }
}