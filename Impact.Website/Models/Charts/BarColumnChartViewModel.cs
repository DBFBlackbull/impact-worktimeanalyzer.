using System.Collections.Generic;
using Impact.Website.Models.Options;

namespace Impact.Website.Models.Charts
{
    public class BarColumnChartViewModel : IChart
    {
        public string DivId { get; set; }
        public bool IsNormalized { get; set; }

        public List<object[]> RawData { get; set; }
        public List<object[]> NormalizedPreviousData { get; set; }
        public List<object[]> NormalizedAllData { get; set; }

        public BarColumnOptions Options { get; set; }
    }
}