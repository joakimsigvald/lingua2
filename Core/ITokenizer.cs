using System.Collections.Generic;

namespace Lingua.Core
{
    using Tokens;

    public interface ITokenizer
    {
        IEnumerable<Token> Tokenize(string text);
    }
}