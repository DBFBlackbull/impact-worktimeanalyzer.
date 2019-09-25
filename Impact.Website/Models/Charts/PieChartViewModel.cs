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
            public ChartAreaViewModel ChartArea { get; set; }
            
            public class ChartAreaViewModel
            {
                public string Left { get; set; }
                public string Top { get; set; }
                public string Width { get; set; }
                public string Height { get; set; }

                public ChartAreaViewModel()
                {
                    Left = "auto";
                    Top = "auto";
                    Width = "auto";
                    Height = "auto";
                }
            }
        }
    }
}