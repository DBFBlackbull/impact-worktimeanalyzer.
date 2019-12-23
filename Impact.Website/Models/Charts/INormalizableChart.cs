using System.Collections.Generic;

namespace Impact.Website.Models.Charts
{
    public interface INormalizableChart : IChartId
    {
        List<object[]> RawData { get; set; }
        List<object[]> NormalizedPreviousData { get; set; }
        List<object[]> NormalizedAllData { get; set; }
    }
}