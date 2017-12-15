using System;

namespace Impact.Core.Model
{
    public class Quarter
    {
        public int Number { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public DateTime MidDate { get; set; }

        public string GetDisplayText()
        {
            switch (Number)
            {
                case 1:
                    return "1. Kvartal";
                case 2:
                    return "2. Kvartal";
                case 3:
                    return "3. Kvartal";
                case 4:
                    return "4. Kvartal";
                default:
                    throw new IndexOutOfRangeException("Quarter was now 1, 2, 3, or 4. Real value: " + Number);
            }
        }
    }
    
    
}
