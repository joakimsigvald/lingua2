using System.Collections.Generic;
using Lingua.Core;

namespace Lingua.Grammar
{
    public interface IEvaluator
    {
        Evaluation Evaluate(ushort[] code);
        Translation[] Arrange(IEnumerable<Translation> translations);
    }
}