using System.Linq;
using Moq;
using NUnit.Framework;

namespace Lingua.Core.Test
{
    [TestFixture]
    public class CapitalizerTests
    {
        [TestCase(new[] { "Hi>Hej" }, new[] { "Hi>Hej" }, new[] { "Hi>Hej" })]
        public void Test(string[] arrangedTranslations, string[] allTranslations, string[] expectedTranslations)
        {
            var arranged = arrangedTranslations.Select(CreateTranslation).ToArray();
            var all = allTranslations.Select(CreateTranslation).ToArray();
            var expected = expectedTranslations.Select(CreateTranslation).ToArray();
            var capitalizer = new Capitalizer();
            var actual = capitalizer.Capitalize(arranged, all);
            Assert.That(actual.Select(t => t.Output), Is.EquivalentTo(expected.Select(t => t.Output)));
        }

        private static ITranslation CreateTranslation(string arg)
        {
            var parts = arg.Split('>');
            return Mock.Of<ITranslation>(
                tran => tran.Input == parts[0] 
                && tran.Output == parts[1]);
        }
    }
}
