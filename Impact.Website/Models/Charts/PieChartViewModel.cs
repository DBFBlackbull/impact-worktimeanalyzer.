using System.Collections.Generic;

namespace Impact.Website.Models.Charts
{
    public class PieChartViewModel : IChart
    {
        public string DivId { get; set; }
        public List<object[]> PreviousWeeks { get; set; }
        public List<object[]> AllWeeks { get; set; }
        public OptionViewModel Options { get; set; }
        
        public class OptionViewModel
        {
            public string Title { get; set; }
            public List<string> Colors { get; set; }
        }
    }
}