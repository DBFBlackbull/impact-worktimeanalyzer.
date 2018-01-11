using System.Collections.Generic;

namespace Impact.Website.Models
{
    public class WeeksChartViewModel : IChart
    {
        public string DivId { get; set; }
        public string GraphTitle { get; set; }
        public bool IsNormalized { get; set; }
        public int YMax { get; set; }

        public List<object[]> Json { get; set; }
        public List<object[]> NormalizedJson { get; set; }
    }
}