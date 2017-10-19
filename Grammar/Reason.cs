using System.Collections.Generic;
using Lingua.Core;

namespace Lingua.Grammar
{
    internal class Reason : IReason
    {
        private readonly List<IEvaluation> _evaluations = new List<IEvaluation>();
        public ushort[] Code { get; internal set; }

        public IEnumerable<IEvaluation> Evaluations => _evaluations;

        public void Add(IEnumerable<IEvaluation> evaluations)
        {
            _evaluations.AddRange(evaluations);
        }
    }
}
