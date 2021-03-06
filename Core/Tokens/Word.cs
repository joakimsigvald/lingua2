﻿using Lingua.Core.Extensions;

namespace Lingua.Core.Tokens
{
    public abstract class Word : Element
    {
        public int VariationIndex
        {
            set => Modifiers |= GetVariationModifier(value);
        }

        protected virtual Modifier GetVariationModifier(int variationIndex) => Modifier.None;

        public bool PossibleAbbreviation { get; set; }

        public override Token Capitalize()
            => Clone(Value.Capitalize());

        public override Token Decapitalize()
            => Clone(Value.Decapitalize());

        public Word Clone(string newValue = null)
        {
            var clone = (Word)MemberwiseClone();
            clone.Value = newValue ?? clone.Value;
            return clone;
        }
    }
}