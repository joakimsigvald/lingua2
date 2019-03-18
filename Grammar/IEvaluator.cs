using Lingua.Core;

namespace Lingua.Grammar
{
    public interface IEvaluator
    {
        byte Horizon { get; }
        IEvaluation Evaluate(ushort[] code, int commonLength = 0);
        int EvaluateInverted(ushort[] invertedCode);
    }
}