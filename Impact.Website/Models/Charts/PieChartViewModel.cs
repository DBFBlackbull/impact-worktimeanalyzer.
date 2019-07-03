using System.Collections.Generic;

namespace Impact.Website.Models.Charts
{
    public class PieChartViewModel : IChart
    {
        public string DivId { get; set; }
        public bool IsNormalized { get; set; }
        
        public List<object[]> RawData { get; set; }
        public List<object[]> NormalizedPreviousData { get; set; }
        public List<object[]> NormalizedAllData { get; set; }
        
        public OptionViewModel Options { get; set; }
        
        public class OptionViewModel
        {
            public string Title { get; set; }
            public List<string> Colors { get; set; }
        }
    }
}