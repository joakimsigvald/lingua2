using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Quantifier : Word
    {
        private string _value;

        public override string Value
        {
            get => _value;
            set
            {
                _value = value; 
                Modifiers = value.ToLower() == "one" ? Modifier.None : Modifier.Plural;
            }
        }
    }
}