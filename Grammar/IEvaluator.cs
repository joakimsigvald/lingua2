using System.Collections.Generic;
using Lingua.Core;

namespace Lingua.Grammar
{
    public interface IEvaluator
    {
        Evaluation Evaluate(ushort[] code, int commonLength = 0);
        ITranslation[] Arrange(IEnumerable<ITranslation> translations);
    }
}