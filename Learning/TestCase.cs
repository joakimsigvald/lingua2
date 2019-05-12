using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Lingua.Translation;

namespace Lingua.Learning
{
    public class TestCase
    {
        public TestCase(string from, string expected)
        {
            From = from;
            Expected = expected;
        }

        public IDecomposition Decomposition { get; set; }
        public TranslationTarget Target => Targets?.FirstOrDefault();
        public TranslationTarget[] Targets { private get; set; }
        public string Suite { get; set; }
        public string From { get; }
        public string Expected { get; }
        public ReductionResult? Reduction { get; set; }
        public TranslationResult? Result{ get; set; }

        public override string ToString() => $"{From}=>{Expected}";

        public string ToSave() => $"\"{From}\", \"{Expected}\"";

        public void RemoveTarget()
        {
            Targets = Targets.Skip(1).ToArray();
        }

        public void PrepareForLearning(ITranslator translator)
        {
            Decomposition = translator.Decompose(From);
            Targets = TargetSelector.SelectTargets(Decomposition, Expected);
            if (Target == null)
                throw new Exception(
                    "Should not get into this state - throw exception from TargetSelector if no possible translation");
        }
    }
}