using System.Collections.Generic;

namespace Impact.Website.Models
{
    public class BalanceChartViewModel : IChart
    {
        public string DivId { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Color { get; set; }
        public int XMax { get; set; }
        public int XMin { get; set; }
        public List<object[]> Json { get; set; }
    }
}