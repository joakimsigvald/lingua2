using Lingua.Core.Tokens;

namespace Lingua.Core
{
    public interface ITokenGenerator
    {
        Token[] GetTokens(string? original);
    }
}