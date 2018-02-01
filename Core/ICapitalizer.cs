﻿using System.Collections.Generic;

namespace Lingua.Core
{
    public interface ICapitalizer
    {
        IEnumerable<ITranslation> Capitalize(IList<ITranslation> arrangedTranslations, IList<ITranslation> allTranslations);
    }
}