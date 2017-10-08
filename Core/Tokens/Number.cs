namespace Lingua.Core.Tokens
{
    public class Number : Element
    {
        public const ushort Code = 3 << Encoder.ModifierBits;

        private string _value;

        public override string Value
        {
            get => _value;
            set
            {
                _value = value; 
                Modifiers = value == "1" ? Modifier.None : Modifier.Plural;
            }
        }
    }
}