using System.Collections.Generic;

namespace Impact.Website.Models.Charts
{
    public class GaugeChartViewModel : IChart
    {
        public string DivId { get; set; }
        public bool IsNormalized { get; set; }
        
        public List<object[]> RawData { get; set; }
        public List<object[]> NormalizedPreviousData { get; set; }
        public List<object[]> NormalizedAllData { get; set; }
        
        public OptionsViewModel Options { get; set; }
        
        public class OptionsViewModel
        {
            public OptionsViewModel(int redFrom, int redTo, int yellowFrom, int yellowTo)
            {
                RedFrom = redFrom;
                RedTo = redTo;
                YellowFrom = yellowFrom;
                YellowTo = yellowTo;
                MinorTicks = 5;
            }

            public int YellowFrom { get; set; }
            public int YellowTo { get; set; }
            public int RedFrom { get; set; }
            public int RedTo { get; set; }
            public int MinorTicks { get; set; }
        }
    }
}