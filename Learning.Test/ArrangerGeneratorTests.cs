using System.Linq;
using Xunit;

namespace Lingua.Learning.Test
{
    using Grammar;

    public class ArrangerGeneratorTests
    {
        [Theory]
        [InlineData("V:1")]
        [InlineData("VN:12")]
        [InlineData("VN:21", "VN:21")]
        [InlineData("VNA:132", "NA:21", "VNA:132")]
        [InlineData("VNAX:1324", "NA:21", "VNA:132", "NAX:213", "VNAX:1324")]
        [InlineData("VNAXV:14325", "NA:21", "AX:21", "NAX:321", "VNAX:1432", "NAXV:3214", "VNAXV:14325")]
        [InlineData("VNV:132", "NV:21", "VNV:132")]
        [InlineData("TN:2", "TN:2")]
        [InlineData("TNV:23", "TN:2", "TNV:23")]
        public void Test(string patternOrder, params string[] expectedOutput)
        {
            var targetArranger = Arrangement.Deserialize(patternOrder);
            var output = ArrangerGenerator.GetArrangerCandidates(new []{ targetArranger });
            var outputArrangers = output.Select(arr => arr.Arrangement.Serialize());
            Assert.Equal(expectedOutput, outputArrangers);
        }
    }
}
