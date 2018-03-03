using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    public class EvaluationProcess
    {
        private readonly ushort[] _sequence;
        private readonly int _commonLength;

        public EvaluationProcess(ushort[] sequence, int commonLength)
        {
            _sequence = sequence;
            _commonLength = commonLength;
        }

        public IEnumerable<ScoreTreeNode> GetMatchingScoreNodes(ScoreTreeNode root) 
            => Enumerable.Range(0, _sequence.Length)
            .SelectMany(i => GetMatchingScorers(root, i));

        private IEnumerable<ScoreTreeNode> GetMatchingScorers(ScoreTreeNode subtree, int index)
        {
            if (index > _commonLength && subtree.Score != 0)
                yield return subtree;
            if (index >= _sequence.Length) yield break;
            var code = _sequence[index];
            var matchingChildren = subtree.GetMatchingChildren(code);
            foreach (var node in GetMatchingScorers(matchingChildren, index + 1))
                yield return node;
        }

        private IEnumerable<ScoreTreeNode> GetMatchingScorers(
            IEnumerable<ScoreTreeNode> matchingChildren, int index)
            => matchingChildren.SelectMany(child => GetMatchingScorers(child, index));
    }
}