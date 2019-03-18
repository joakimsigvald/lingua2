using Lingua.Core;
using Lingua.Core.Tokens;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Lingua.Grammar.Test
{
    public class NewGrammarEngineTest
    {
        private const ushort Code1 = 1;
        private const ushort Code2 = 2;
        private const ushort Code3 = 3;
        private const ushort CodeDoubleWord = 9;

        private static readonly ITranslation Translation1 = MockTranslation(Code1);
        private static readonly ITranslation Translation2 = MockTranslation(Code2);
        private static readonly ITranslation Translation3 = MockTranslation(Code3);
        private static readonly ITranslation DoubleWord = MockTranslation(CodeDoubleWord, 2);

        [Fact]
        public void When_empty_possiblities_then_return_empty_translations()
        {
            Test(MockEvaluator(), new List<ITranslation[]>());
        }

        [Fact]
        public void When_single_possiblities_then_return_that_translation()
        {
            Test(MockEvaluator(), new [] { new[] { Translation1 } }, Translation1);
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
            var evaluator = MockEvaluator(
                evaluations: new[] { (new[] { Code1 }, 1), (new[] { Code2 }, 2) });
            Test(evaluator, new[] {
                new[] { Translation1, Translation2 } },
                Translation2);
        }

        [Fact]
        public void When_two_paths_return_best_path()
        {
            var evaluator = MockEvaluator(
                evaluations: new[] { (new[] { Code2, Code1 }, 1), (new[] { Code3, Code2 }, 2) });
            Test(evaluator, new[] {
                new[] { Translation1, Translation2 }, new[] { Translation2, Translation3 } },
                Translation2, Translation3);
        }

        [Fact]
        public void When_two_paths_one_with_compound_word_then_return_best_path()
        {
            var evaluator = MockEvaluator(
                evaluations: (new[] { CodeDoubleWord, Code1 }, 1));
            Test(evaluator, new[] {
                new[] { Translation1, Translation2 },
                new[] { Translation3, DoubleWord },
                new[] { Translation3 } },
                Translation1, DoubleWord);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void When_very_long_paths_only_evaluate_path_within_horizon(int longCount)
        {
            var evaluator = MockEvaluator(2, (new[] { CodeDoubleWord, Code1 }, 1));
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

        private static ITranslation MockTranslation(ushort code, byte wordCount = 1)
        {
            var mock = new Mock<ITranslation>();
            mock.Setup(t => t.Code).Returns(code);
            mock.Setup(t => t.WordCount).Returns(wordCount);
            mock.Setup(t => t.From).Returns(new AnyToken());
            mock.Setup(t => t.ToString()).Returns($"{code}");
            return mock.Object;
        }

        private IEvaluator MockEvaluator(byte horizon = byte.MaxValue, params (ushort[] code, int score)[] evaluations)
        {
            var mock = new Mock<IEvaluator>();
            mock.Setup(eval => eval.Horizon).Returns(horizon);
            mock.Setup(eval => eval.EvaluateInverted(It.IsAny<ushort[]>())).Returns(0);
            foreach (var (code, score) in evaluations)
            {
                mock.Setup(eval => eval.EvaluateInverted(code))
                    .Returns(score);
            }
            return mock.Object;
        }

        private void Test(IEvaluator evaluator, IList<ITranslation[]> possibilities, params ITranslation[] expected)
        {
            Test(new NewGrammarEngine(evaluator), possibilities, expected);
        }

        private void Test(IGrammar grammar, IList<ITranslation[]> possibilities, params ITranslation[] expected)
        {
            var (result, _) = grammar.Reduce(possibilities);
            Assert.Equal(expected, result);
        }
    }
}