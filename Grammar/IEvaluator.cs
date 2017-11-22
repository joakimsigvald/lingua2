namespace Lingua.Grammar
{
    public interface IEvaluator
    {
        Evaluation Evaluate(ushort[] code);
    }
}