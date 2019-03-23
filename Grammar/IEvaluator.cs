using Lingua.Core;

namespace Lingua.Grammar
{
    public interface IEvaluator
    {
        byte Horizon { get; }
        int EvaluateReversed(ushort[] invertedCode);
    }
}