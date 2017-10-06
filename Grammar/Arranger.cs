using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Lingua.Core.Tokens;

namespace Lingua.Grammar
{
    internal class Arranger
    {
        public Arranger(string from, string to)
        {
            From = Encoder.Encode(Encoder.Deserialize(from)).ToArray();
            To = to.Select(c => (byte)(c - 49)).ToArray();
        }

        private ushort[] From { get; }
        private byte[] To { get; }

        public IEnumerable<Translation> Arrange(IList<Translation> input)
        {
            var segLen = From.Length * 2 - 1;
            for (var i = 0; i < input.Count; i++)
            {
                var segment = input
                    .Skip(i).Take(segLen)
                    .ToArray();
                var tokens = segment.Select(t => t.From).ToArray();
                var codedSegment = Encoder.Encode(tokens).ToArray();
                if (Encoder.Matches(codedSegment, From))
                {
                    var dividers = segment
                        .Where(t => t.From is Divider)
                        .ToArray();
                    var words = segment
                        .Except(dividers)
                        .ToArray();
                    var rearrangedSegment = Rearrange(words).ToList()
                        .Interleave(dividers.Take(1).ToArray(), true);
                    foreach (var tran in rearrangedSegment)
                        yield return tran;
                    i += segLen - 1;
                }
                else yield return input[i];
            }
        }

        private IEnumerable<Translation> Rearrange(IList<Translation> segment)
            => To.Select(i => segment[i]);
    }
}