using System.Collections.Generic;

namespace Impact.Website.Models.Charts
{
    public interface IChart
    {
        string DivId { get; set; }
        bool IsNormalized { get; set; }

        List<object[]> RawData { get; set; }
        List<object[]> NormalizedPreviousData { get; set; }
        List<object[]> NormalizedAllData { get; set; }
    }
}