using System.Collections.Generic;

namespace Impact.Website.Models
{
    public class WeeksViewModel
    {
        public string GraphTitle { get; set; }
        public bool IsNormalized { get; set; }

        public List<object[]> Json { get; set; }
        public List<object[]> NormalizedJson { get; set; }
    }
}