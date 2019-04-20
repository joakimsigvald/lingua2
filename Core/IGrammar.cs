﻿using System.Collections.Generic;

namespace Lingua.Core
{
    public interface IGrammar
    {
        ReductionResult Reduce(IList<IGrammaton[]> possibilities);
        ReductionResult Evaluate(IGrammaton[] grammatons);
    }
}