﻿using System.Collections.Generic;
using Lingua.Core.Tokens;

namespace Lingua.Core.WordClasses
{
    public class Article : Word
    {
        public Article()
        {
        }

        public Article(Modifier modifiers)
            => Modifiers = modifiers;

        protected override IEnumerable<Modifier> GetIndividualModifiers(int variationIndex)
        {
            if ((variationIndex & 1) > 0)
                yield return Modifier.Qualified;
            if ((variationIndex & 2) > 0)
                yield return Modifier.Plural;
        }
    }
}