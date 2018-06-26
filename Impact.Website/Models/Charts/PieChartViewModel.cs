using System.Collections.Generic;

namespace Impact.Website.Models.Charts
{
    public class PieChartViewModel : IChart
    {
        public string DivId { get; set; }
        public bool IsNormalized { get; set; }
        
        public List<object[]> RawWeeks { get; set; }
        public List<object[]> NormalizedPreviousWeeks { get; set; }
        public List<object[]> NormalizedAllWeeks { get; set; }
        
        public OptionViewModel Options { get; set; }
        
        public class OptionViewModel
        {
            public string Title { get; set; }
            public List<string> Colors { get; set; }
        }
    }
}