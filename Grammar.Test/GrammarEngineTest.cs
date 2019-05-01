using Lingua.Core;
using Lingua.Core.Tokens;
using Lingua.Learning;
using Lingua.Tokenization;
using Lingua.Translation;
using Lingua.Vocabulary;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Lingua.Grammar.Test
{
    public class GrammarEngineTest
    {
        private const ushort Code1 = 1;
        private const ushort Code2 = 2;
        private const ushort Code3 = 3;
        private const ushort CodeDoubleWord = 9;

        private static readonly IGrammaton Translation1 = MockTranslation(Code1);
        private static readonly IGrammaton Translation2 = MockTranslation(Code2);
        private static readonly IGrammaton Translation3 = MockTranslation(Code3);
        private static readonly IGrammaton DoubleWord = MockTranslation(CodeDoubleWord, 2);

        [Fact]
        public void When_empty_possiblities_then_return_empty_translations()
        {
            Test(MockEvaluator(), new List<IGrammaton[]>());
        }

        [Fact]
        public void When_single_possiblities_then_return_that_translation()
        {
            Test(MockEvaluator(), new[] { new[] { Translation1 } }, Translation1);
        }

        [Fact]
        public void When_unbranching_possiblities_then_return_those_translations()
        {
            Test(MockEvaluator(), new[] { new[] { Translation1 }, new[] { Translation2 } }, Translation1, Translation2);
        }

        [Fact]
        public void When_translation_contain_double_word_then_skip_next_translation()
        {
            Test(MockEvaluator(), new[] {
                new[] { Translation1 },
                new[] { DoubleWord },
                new[] { Translation2 },
                new[] { Translation3 } },
                Translation1, DoubleWord, Translation3);
        }

        [Fact]
        public void When_two_alternatives_return_translation_with_highest_value()
        {
            var evaluator = MockEvaluator(bestPattern: new[] { Code2 });
            Test(evaluator, new[] {
                new[] { Translation1, Translation2 } },
                Translation2);
        }

        [Fact]
        public void When_two_paths_return_best_path()
        {
            var evaluator = MockEvaluator(bestPattern: new[] { Code2, Code3 });
            Test(evaluator, new[] {
                new[] { Translation1, Translation2 }, new[] { Translation2, Translation3 } },
                Translation2, Translation3);
        }

        [Fact]
        public void When_two_paths_one_with_compound_word_then_return_best_path()
        {
            var evaluator = MockEvaluator(bestPattern: new[] { Code1, CodeDoubleWord });
            Test(evaluator, new[] {
                new[] { Translation1, Translation2 },
                new[] { Translation3, DoubleWord },
                new[] { Translation3 } },
                Translation1, DoubleWord);
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData(2, 2)]
        [InlineData(10, 2)]
        [InlineData(10, 5)]
        public void When_very_long_paths_only_evaluate_path_within_horizon(int longCount, byte horizon)
        {
            var bestPattern = Enumerable.Range(0, horizon - 1).Select(_ => Code1).Append(CodeDoubleWord).ToArray();
            var evaluator = MockEvaluator(horizon, bestPattern);
            var possibilities = Enumerable.Range(0, longCount)
                .Select(_ => new[] { Translation1, Translation2 })
                .Concat(new[] { new[] { Translation3, DoubleWord }, new[] { Translation3 } })
                .ToArray();
            var expected = Enumerable.Range(0, longCount)
                .Select(_ => Translation1)
                .Append(DoubleWord)
                .ToArray();
            Test(evaluator, possibilities, expected);
        }

        [Fact]
        public void EvaluateAndReduceGiveSameScore1()
        {
            var sentence = "I paint the ball";
            var patterns = new Dictionary<string, sbyte> {
                { "V1*", 1},
                { "VT*", 1},
                { "N", -1},
            };
            TestEvaluateAndReduceGiveSameScore(sentence, patterns);
        }

        [Fact]
        public void EvaluateAndReduceGiveSameScore2()
        {
            var sentence = "Bouncing ball to play with";
            var patterns = new Dictionary<string, sbyte> { { "I*", 1 } };
            TestEvaluateAndReduceGiveSameScore(sentence, patterns);
        }

        [Fact]
        public void EvaluateAndReduceGiveSameScore3()
        {
            var sentence = "The fastest cars";
            var patterns = new Dictionary<string, sbyte>
            {
                { "^Tn*", 1},
                { "Ad*", 1},
                { "Nd*", 1},
                { "Tq*", 1 }
            };
            TestEvaluateAndReduceGiveSameScore(sentence, patterns);
        }

        [Fact]
        public void EvaluateAndReduceGiveSameScore4()
        {
            var sentence = "The fastest car";
            var patterns = new Dictionary<string, sbyte>
            {
                { "^Td*", 1},
                { "Tq*", 1 }
            };
            TestEvaluateAndReduceGiveSameScore(sentence, patterns);
        }

        [Fact]
        public void EvaluateAndReduceGiveSameScore5()
        {
            var sentence = "the concert hall.";
            var patterns = new Dictionary<string, sbyte>
            {
                { "D*.", 1}
            };
            TestEvaluateAndReduceGiveSameScore(sentence, patterns);
        }

        private void TestEvaluateAndReduceGiveSameScore(string sentence, Dictionary<string, sbyte> patterns)
        {
            var grammar = CreateGrammar(patterns);
            var possibilities = Decompose(grammar, sentence);
            AssertEvaluateAndReduceGiveSameScore(grammar, possibilities);
        }

        private void AssertEvaluateAndReduceGiveSameScore(IGrammar grammar, IList<IGrammaton[]> possibilities)
        {
            var reduction = grammar.Reduce(possibilities);
            var evaluation = grammar.Evaluate(reduction.Grammatons).Score;
            Assert.Equal(evaluation, reduction.Score);
        }

        private IList<IGrammaton[]> Decompose(IGrammar grammar, string sentence)
        {
            var translator = new Translator(new TokenGenerator(new Tokenizer()), new Thesaurus(), grammar, new Rearranger(), new SynonymResolver(), new Capitalizer());
            return translator.Decompose(sentence);
        }

        private IGrammar CreateGrammar(Dictionary<string, sbyte> patterns)
        {
            var evaluator = Evaluator.Create(patterns);
            return new GrammarEngine(evaluator);
        }

        [Theory]
        [InlineData(
            9,
            "He took the ball with the blue dot and kicked it",
            "Han tog bollen med den blåa pricken och sparkade den")]
        [InlineData(
            9,
            "He took the table with the blue dot and kicked it", 
            "Han tog bordet med den blåa pricken och sparkade det")]
        public void TestEvaluateWithCondensation(
            byte expectedCondensedCodeCount,
            string translateFrom,
            string translateTo)
        {
            var testCase = new TestCase(translateFrom, translateTo);
            var evaluator = Evaluator.Create();
            var grammar = new GrammarEngine(evaluator);
            var translator = new Translator(new TokenGenerator(new Tokenizer()), new Thesaurus(), grammar, new Rearranger(), new SynonymResolver(), new Capitalizer());
            testCase.PrepareForLearning(translator);
            var grammatons = ExtractGrammatons(testCase).ToArray();
            var reduction = grammar.Evaluate(grammatons);
            Assert.Equal(expectedCondensedCodeCount, reduction.CondensedCode.Length);
        }

        private IEnumerable<IGrammaton> ExtractGrammatons(TestCase testCase)
            => testCase.Target.Translations.Select(t => new Grammaton(t));

        private static IGrammaton MockTranslation(ushort code, byte wordCount = 1)
        {
            var mock = new Mock<IGrammaton>();
            mock.Setup(t => t.Code).Returns(code);
            mock.Setup(t => t.WordCount).Returns(wordCount);
            //mock.Setup(t => t.From).Returns(new AnyToken());
            mock.Setup(t => t.ToString()).Returns($"{code}");
            return mock.Object;
        }

        private IEvaluator MockEvaluator(byte horizon = byte.MaxValue, params ushort[] bestPattern)
        {
            var mock = new Mock<IEvaluator>();
            mock.Setup(eval => eval.Horizon).Returns(horizon);
            mock.Setup(eval => eval.ScorePatternsEndingWith(It.IsAny<ushort[]>())).Returns(0);
            var bestPatternReversed = bestPattern.Reverse().ToArray();
            mock.Setup(eval => eval.ScorePatternsEndingWith(
                It.Is<ushort[]>(c => c.Take(bestPattern.Length).SequenceEqual(bestPatternReversed)))).Returns(1);
            return mock.Object;
        }

        private void Test(IEvaluator evaluator, IList<IGrammaton[]> possibilities, params IGrammaton[] expected)
        {
            Test(new GrammarEngine(evaluator), possibilities, expected);
        }

        private void Test(IGrammar grammar, IList<IGrammaton[]> possibilities, params IGrammaton[] expected)
        {
            var result = grammar.Reduce(possibilities);
            Assert.Equal(expected, result.Grammatons);
        }
    }
}