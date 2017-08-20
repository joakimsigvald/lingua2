using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public abstract class Article : Word
    {
        protected Article(Modifier modifiers)
           => Modifiers = modifiers;
    }
}