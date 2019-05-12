using System.Collections.Generic;

namespace Lingua.Core
{
    public interface IDecomposition : IEnumerable<IWordSpace>
    {
        bool IsEmpty { get; }
        int Length { get; }

        IWordSpace this[int index] {get;}
    }
}