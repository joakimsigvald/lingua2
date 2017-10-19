using System.Collections.Generic;

namespace Lingua.Core
{
    public interface IReason
    {
        IEnumerable<IEvaluation> Evaluations { get; }
        ushort[] Code { get; }
    }
}