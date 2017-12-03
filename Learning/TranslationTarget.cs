using System;
using System.Collections.Generic;

namespace Lingua.Learning
{
    using Core;
    using Grammar;

    public class TranslationTarget
    {
        public Translation[] Translations { get; set; }
        public Arrangement Arrangement { get; set; }

        public IEnumerable<Translation> ArrangedTranslations
            => LazyArrangedTranslations.Value;

        private Lazy<IEnumerable<Translation>> LazyArrangedTranslations
            => new Lazy<IEnumerable<Translation>>(ComputeArrangedTranslations);

        private IEnumerable<Translation> ComputeArrangedTranslations()
            => new Arranger(Arrangement).Arrange(Translations);
    }
}