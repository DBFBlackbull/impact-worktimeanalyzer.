using System.Collections.Generic;

namespace Impact.Website.Models.Charts
{
    public class BalanceChartViewModel : IChart
    {
        public BalanceChartViewModel(string divId, List<object[]> previousWeeks, List<object[]> allWeeks = null)
        {
            DivId = divId;
            PreviousWeeks = previousWeeks;
            AllWeeks = allWeeks;
        }

        public string DivId { get; set; }
        public List<object[]> PreviousWeeks { get; set; }
        public List<object[]> AllWeeks { get; set; }
        public OptionsViewModel Options { get; set; }

        public class OptionsViewModel
        {
            public OptionsViewModel(string color, int xMax)
            {
                Height = 170;
                Colors = new List<string> {color};
                Bars = "horizontal";
                Chart = new ChartViewModel();
                HAxis = new HAxisViewModel(xMax);
            }

            public int Height { get; set; }
            public List<string> Colors { get; set; }
            public string Bars { get; set; }
            public ChartViewModel Chart { get; set; }
            public HAxisViewModel HAxis { get; set; }

            public class ChartViewModel
            {
                public string Title { get; set; }
                public string Subtitle { get; set; }
            }

            public class HAxisViewModel
            {
                public HAxisViewModel(int xMax)
                {
                    ViewWindowMode = "explicit";
                    ViewWindow = new ViewWindowViewModel(xMax);
                }

                public string ViewWindowMode { get; set; }
                public ViewWindowViewModel ViewWindow { get; set; }

                public class ViewWindowViewModel
                {
                    public ViewWindowViewModel(int max)
                    {
                        Max = max;
                        Min = max * -1;
                    }

                    public int Max { get; set; }
                    public int Min { get; set; }
                }
            }
        }
    }
}