using System.Collections.Generic;

namespace Lingua.Core
{
    public interface IReason
    {
        IList<IEvaluation> Evaluations { get; }
        ushort[] Code { get; }
        string Pattern { get; }
    }
}