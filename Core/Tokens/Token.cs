﻿namespace Lingua.Core.Tokens
{
    public abstract class Token
    {
        public virtual string Value { get; set; }

        public virtual Token Capitalize() => this;

        public override string ToString()
            => $"{Encoder.Serialize(this)}: {Value}";
    }
}