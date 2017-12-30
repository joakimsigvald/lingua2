using System;
using System.Collections.Generic;
using Lingua.Core;

namespace Lingua.Learning
{
    public class MissingTranslations : Exception
    {
        public MissingTranslations(string partiallyTargetedTranslation, IEnumerable<ITranslation> untranslated)
            : base($"Failed to translate {partiallyTargetedTranslation}. {string.Join(", ", untranslated)}")
        {
        }
    }
}
