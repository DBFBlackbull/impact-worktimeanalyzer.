using System;
using System.Collections.Generic;
using Impact.Core.Constants;

namespace Impact.Website.Helper.Comparers
{
    /// <summary>
    /// A name comparer that always ensures that Impact is placed last.
    /// This is because impact is the least important name in this context
    /// </summary>
    public class CustomerNameComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == ApplicationConstants.ImpactName /*|| x == ApplicationConstants.OtherName*/)
                return 1;

            if (y == ApplicationConstants.ImpactName /*|| y == ApplicationConstants.OtherName*/)
                return -1;

            return string.Compare(x, y, StringComparison.Ordinal);
        }
    }
}