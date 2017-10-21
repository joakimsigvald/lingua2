using System.Collections.Generic;
using System.Linq;

namespace Lingua.Testing
{
    using Core;
    using Grammar;
    using Tokenization;
    using Vocabulary;

    public class Trainer
    {
        private readonly TrainableEvaluator _evaluator;
        private readonly ITranslator _translator;
        private readonly ITokenizer _tokenizer;
        private TestRunner _testRunner;

        public Trainer()
        {
            _evaluator = new TrainableEvaluator();
            _tokenizer = new Tokenizer();
            _translator = new Translator(_tokenizer, new Thesaurus(), new Engine(_evaluator));
        }

        public (TestCaseResult, int) RunTrainingSession()
        {
            _testRunner = new TestRunner(_translator, _tokenizer, true);
            IEnumerator<string> addPatterns = new List<string>().GetEnumerator();
            var runTestsCount = 0;
            string currentPattern = null;
            TestCaseResult[] results;
            while (!(results = _testRunner.RunTestCases()).All(result => result.Success))
            {
                var failedCase = results.Last();
                if (results.Length > runTestsCount)
                {
                    currentPattern = null;
                    addPatterns.Dispose();
                    addPatterns = GetPossiblePatternsToAdd(failedCase);
                    runTestsCount = results.Length;
                }
                do
                {
                    _evaluator.DownPattern(currentPattern);
                    if (!addPatterns.MoveNext())
                        return (failedCase, runTestsCount);
                    currentPattern = addPatterns.Current;
                    _evaluator.UpPattern(currentPattern);
                    failedCase = _testRunner.RunTestCase(failedCase.Group, failedCase.From,
                        failedCase.Expected);
                } while (!failedCase.Success);
            }
            addPatterns.Dispose();
            return (null, runTestsCount);
        }

        private static IEnumerator<string> GetPossiblePatternsToAdd(TestCaseResult result)
            => YieldPossiblePatternsToAdd(result).GetEnumerator();

        private static IEnumerable<string> YieldPossiblePatternsToAdd(TestCaseResult result)
        {
            var codes = YieldPossibleCodesToAdd(result).Distinct().ToArray();
            var generalizedCodes = codes.Select(code => (ushort)(Encoder.ModifiersMask | code)).Distinct().ToArray();
            return generalizedCodes.Concat(codes).Select(code => Encoder.Serialize(new[] { code }));
        }

        private static IEnumerable<ushort> YieldPossibleCodesToAdd(TestCaseResult result) 
            => result.ExpectedCandidates.SelectMany(c => c.Select(t => Encoder.Encode(t.From))).ToArray();
    }
}