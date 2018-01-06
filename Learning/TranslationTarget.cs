using System;
using System.Collections.Generic;

namespace Lingua.Learning
{
    using Core;
    using Grammar;

    public class TranslationTarget
    {
        public ITranslation[] Translations { get; set; }
        public Arrangement Arrangement { get; set; }
        public string Unmatched { get; set; }

        public IEnumerable<ITranslation> ArrangedTranslations
            => LazyArrangedTranslations.Value;

        private Lazy<IEnumerable<ITranslation>> LazyArrangedTranslations
            => new Lazy<IEnumerable<ITranslation>>(ComputeArrangedTranslations);


        private IEnumerable<ITranslation> ComputeArrangedTranslations()
            => new Arranger(Arrangement).Arrange(Translations);
    }
}