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
                Modifiers = GetModifiers(value);
            }
        }

        protected virtual Modifier GetModifiers(int variationIndex) => Modifiers;

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