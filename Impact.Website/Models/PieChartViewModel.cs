using System.Collections.Generic;

namespace Impact.Website.Models
{
    public class PieChartViewModel : IChart
    {
        public string DivId { get; set; }
        public string Title { get; set; }
        public List<object[]> Json { get; set; }
    }
}