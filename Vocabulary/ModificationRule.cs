using System;
using Lingua.Core;
using Lingua.Core.Tokens;

namespace Lingua.Vocabulary
{
    public interface IModificationRule
    {
        Translation Apply(Translation translation);
    }

    public class ModificationRule<TWord> : IModificationRule
        where TWord : Word
    {
        private readonly Modifier _modifier;
        private string[] _translateFrom;
        private string[] _translateTo;

        public ModificationRule(Modifier modifier, string[] translateFrom, string[] translateTo)
        {
            _modifier = modifier;
            _translateFrom = translateFrom;
            _translateTo = translateTo;
        }

        public Translation Apply(Translation translation)
        {
            var modifiedFrom = (translation.From as TWord)?.Clone(translation.From.Value + "'s");
            if (modifiedFrom == null) return null;
            modifiedFrom.Modifiers |= _modifier;
            var modifiedTo = translation.To + "s";
            return Translation.Create(modifiedFrom, modifiedTo);
        }
    }
}