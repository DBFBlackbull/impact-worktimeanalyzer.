using System.Collections.Generic;

namespace Impact.Website.Models
{
    public class GaugeChartViewModel : IChart
    {
        public GaugeChartViewModel(string divId, OptionsViewModel options, List<object[]> json)
        {
            DivId = divId;
            Json = json;
            Options = options;
        }

        public string DivId { get; set; }
        public List<object[]> Json { get; set; }
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