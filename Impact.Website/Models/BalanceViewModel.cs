using System.Collections.Generic;

namespace Impact.Website.Models
{
    public class BalanceViewModel
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Color { get; set; }
        public List<object[]> Json { get; set; }
    }
}