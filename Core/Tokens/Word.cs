using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core.Tokens
{
    public abstract class Word : Element
    {
        private int _variationIndex;

        public int VariationIndex
        {
            private get { return _variationIndex; }
            set
            {
                _variationIndex = value;
                Modifiers |= GetAdditionalModifiers(value);
            }
        }

        private Modifier GetAdditionalModifiers(int variationIndex)
            => GetIndividualModifiers(variationIndex)
                .Aggregate(Modifier.None, (first, second) => first | second);

        protected virtual IEnumerable<Modifier> GetIndividualModifiers(int variationIndex)
        {
            yield break;
        }

        public bool PossibleAbbreviation { get; set; }

        public override Token Capitalize()
            => Clone(Value.Capitalize());

        public Word Clone(string newValue = null)
        {
            var clone = (Word)MemberwiseClone();
            clone.Value = newValue ?? clone.Value;
            return clone;
        }
    }
}