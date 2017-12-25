using System.Collections.Generic;
using System.Linq;
using Lingua.Core;

namespace Lingua.Grammar
{
    public class EvaluationProcess
    {
        private readonly ushort[] _sequence;

        public EvaluationProcess(ushort[] sequence) => _sequence = sequence;

        public IEnumerable<ScoreTreeNode> GetMatchingScoreNodes(ScoreTreeNode root)
        {
            return Enumerable.Range(0, _sequence.Length)
                .SelectMany(i => GetMatchingScorers(root, i));
        }

        private IEnumerable<ScoreTreeNode> GetMatchingScorers(ScoreTreeNode subtree, int index)
        {
            if (subtree.Score != 0) yield return subtree;
            if (index >= _sequence.Length) yield break;
            var code = _sequence[index];
            var matchingChildren = subtree.GetMatchingChildren(code);
            foreach (var node in GetMatchingScorers(matchingChildren, index + 1))
                yield return node;
        }

        private IEnumerable<ScoreTreeNode> GetMatchingScorers(IEnumerable<ScoreTreeNode> matchingChildren,
            int index)
            => matchingChildren.SelectMany(child => GetMatchingScorers(child, index));
    }
}