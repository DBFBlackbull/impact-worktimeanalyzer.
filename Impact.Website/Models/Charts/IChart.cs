using System.Collections.Generic;

namespace Impact.Website.Models.Charts
{
    public interface IChart
    {
        string DivId { get; set; }
        bool IsNormalized { get; set; }

        List<object[]> RawWeeks { get; set; }
        List<object[]> NormalizedPreviousWeeks { get; set; }
        List<object[]> NormalizedAllWeeks { get; set; }
    }
}