using System.Collections.Generic;
using System.Linq;
using Lingua.Core;

namespace Lingua.Grammar
{
    internal class Reason : IReason
    {
        public Reason(ushort[] code, IEnumerable<IEvaluation> evaluations)
        {
            Code = code;
            Evaluations = Filter(evaluations).ToArray();
        }

        public ushort[] Code { get; }

        public IList<IEvaluation> Evaluations { get; }

        public string Pattern => Encoder.Serialize(Code);

        private static IEnumerable<IEvaluation> Filter(IEnumerable<IEvaluation> evaluations)
            => evaluations.Where(ev => ev.Score != 0).Distinct();
    }
}
