namespace Impact.Website.Models
{
    public class SummedViewModel
    {
        public string PayoutMonth { get; set; }
        
        // public Data RawAll { get; set; }
        public Data NormalizedPrevious { get; set; }
        public Data NormalizedAll { get; set; }
        public decimal NormalWorkMonth { get; set; }

        public class Data
        {
            public decimal Flex0 { get; set; } 
            public decimal Flex100 { get; set; } 
            public decimal Payout { get; set; } 
        }
    }
}