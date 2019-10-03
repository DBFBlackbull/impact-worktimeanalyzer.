using System.Collections.Generic;
using Impact.Website.Models.Charts;

namespace Impact.Website.Models.Options
{
    public abstract class BarColumnOptions
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<string> Colors { get; set; }
        public AxisViewModel VAxis { get; set; }
        public AxisViewModel HAxis { get; set; }
        public bool IsStacked { get; set; }
        
        public class OptionsViewModel : BarColumnOptions
        {
            public string Title { get; set; }
            public BarViewModel Bar { get; set; }
            public AnimationViewModel Animation { get; set; }
            public SeriesViewModel Series { get; set; }

            public class BarViewModel
            {
                public int GroupWidth { get; set; }
            }

            public class AnimationViewModel
            {
                public int Duration { get; set; }
                public AnimationEasing Easing { get; set; }
            }
            
            public class SeriesViewModel
            {
                public ZeroViewModel Zero { get; set; }
                
                public class ZeroViewModel
                {
                    public string Type { get; set; }
                }
            }
        }
        
        public class MaterialOptionsViewModel : BarColumnOptions
        {
            public BarOrientation Bars { get; set; }
            public ChartViewModel Chart { get; set; }

            public class ChartViewModel
            {
                public string Title { get; set; }
                public string Subtitle { get; set; }
            }
        }

        public class AxisViewModel
        {
            public string Title { get; set; }
            public List<decimal> Ticks { get; set; }
            public ViewWindowViewModel ViewWindow { get; set; }
                
            public class ViewWindowViewModel
            {
                public int Max { get; set; }
                public int Min { get; set; }
            }
        }
    }
}