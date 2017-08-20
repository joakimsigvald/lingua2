namespace Lingua.Core.Tokens
{
    public abstract class Token
    {
        public virtual string Value { get; set; }

        public override string ToString()
            => $"{GetType().Name}: {Value}";

        public virtual Token Capitalize() => this;
    }
}