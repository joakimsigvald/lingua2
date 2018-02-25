using System.Collections.Generic;
using System.Linq;
using Lingua.Core;

namespace Lingua.Learning
{
    public class TestCase
    {
        public TestCase(string from, string expected)
        {
            From = from;
            Expected = expected;
        }

        public IList<ITranslation[]> Possibilities { get; set; }
        public TranslationTarget Target => Targets?.FirstOrDefault();
        public TranslationTarget[] Targets { private get; set; }
        public string Suite { get; set; }
        public string From { get; }
        public string Expected { get; }
        public ITranslation[] Reduction { get; set; }
        public TranslationResult Result{ get; set; }

        public override string ToString()
            => $"{From}=>{Expected}";

        public void RemoveTarget()
        {
            Targets = Targets.Skip(1).ToArray();
        }
    }
}