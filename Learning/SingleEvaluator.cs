using Lingua.Core;

namespace Lingua.Learning
{
    internal class SingleEvaluator
    {
        private Code code;
        private int v;

        public SingleEvaluator(Code code, int v)
        {
            this.code = code;
            this.v = v;
        }
    }
}