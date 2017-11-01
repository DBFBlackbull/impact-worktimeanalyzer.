using System;

namespace Impact.Core.Model
{
    public class Quarter
    {
        public int Number { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public DateTime MidDate { get; set; }
    }
}
