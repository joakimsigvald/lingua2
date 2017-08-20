namespace Lingua.Core.Tokens
{
    public class Ellipsis : Token
    {
        public bool Shortened { get; set; }
        public override string Value => Shortened ? ".." : "...";
    }
}