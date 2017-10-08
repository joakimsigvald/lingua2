using System.Collections.Generic;

namespace Lingua.Core
{
    public interface IEvaluation
    {
        IList<ushort[]> Patterns { get; }
        int Score { get; }
    }
}