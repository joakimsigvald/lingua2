using System.Linq;
using NUnit.Framework;

namespace Lingua.Learning.Test
{
    using Grammar;

    [TestFixture]
    public class ArrangerGeneratorTests
    {
        [TestCase("V:1")]
        [TestCase("VN:21", "VN:21")]
        [TestCase("VNA:132", "NA:21", "VNA:132")]
        [TestCase("VNAX:1324", "NA:21", "VNA:132", "NAX:213", "VNAX:1324")]
        [TestCase("VNAXV:14325", "NA:21", "AX:21", "NAX:321", "VNAX:1432", "NAXV:3214", "VNAXV:14325")]
        [TestCase("VNV:132", "NV:21", "VNV:132")]
        [TestCase("TN:2", "TN:2")]
        [TestCase("TNV:23", "TN:2", "TNV:23")]
        public void Test(string patternOrder, params string[] expectedOutput)
        {
            var targetArranger = Arrangement.Deserialize(patternOrder);
            var output = ArrangerGenerator.GetArrangerCandidates(new []{ targetArranger });
            var outputArrangers = output.Select(arr => arr.Serialize());
            Assert.That(outputArrangers, Is.EqualTo(expectedOutput));
        }
    }
}
