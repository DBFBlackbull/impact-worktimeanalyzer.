using System.Collections.Generic;
using Impact.Website.Models.Options;

namespace Impact.Website.Models.Charts
{
    public class BarColumnChartViewModel : IChart
    {
        public string DivId { get; set; }
        public bool IsNormalized { get; set; }

        public List<object[]> RawWeeks { get; set; }
        public List<object[]> NormalizedPreviousWeeks { get; set; }
        public List<object[]> NormalizedAllWeeks { get; set; }

        public BarColumnOptions Options { get; set; }
    }
}