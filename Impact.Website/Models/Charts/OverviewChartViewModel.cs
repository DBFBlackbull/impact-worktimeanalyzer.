using System.Collections.Generic;

namespace Impact.Website.Models.Charts
{
    public class OverviewChartViewModel : IChart
    {
        public string DivId { get; set; }
        public string GraphTitle { get; set; }
        public bool IsNormalized { get; set; }
        public int YMax { get; set; }

        public List<object[]> Json { get; set; }
        public List<object[]> NormalizedJson { get; set; }

        public OptionsViewModel Options { get; set; }

        public class OptionsViewModel
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public ChartViewModel Chart { get; set; }
            public AnimationViewModel Animation { get; set; }
            public VAxisViewModel VAxis { get; set; }

            public class ChartViewModel
            {
                public string Title { get; set; }
                public string Subtitle { get; set; }
            }
            
            public class AnimationViewModel 
            {
                public int Duration { get; set; }
                public string Easing { get; set; }
            }
            
            public class VAxisViewModel 
            {
                public string ViewWindowMode { get; set; }
                public ViewWindowViewModel ViewWindow { get; set; }
                
                public class ViewWindowViewModel
                {
                    public int Max { get; set; }
                    public int Min { get; set; }
                }
            }
        }
    }
}