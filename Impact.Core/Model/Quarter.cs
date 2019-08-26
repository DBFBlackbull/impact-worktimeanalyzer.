using System;

namespace Impact.Core.Model
{
    public class Quarter
    {
        public int Number { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public DateTime MidDate { get; set; }

        public string GetDisplayTitle()
        {
            switch (Number)
            {
                case 1:
                    return "1. Kvartal: Januar til Marts";
                case 2:
                    return "2. Kvartal: April til Juni";
                case 3:
                    return "3. Kvartal: Juli til September";
                case 4:
                    return "4. Kvartal: Oktober til December";
                default:
                    throw new IndexOutOfRangeException("Quarter was now 1, 2, 3, or 4. Real value: " + Number);
            }
        }
        
        public string GetDisplayOvertimePayoutMonth()
        {
            // The payout month got changed at this specific date. See "PdfRegistreringer/Fortroligt Information vedr udbetalingstidspunkt for overarbejde.pdf"
            if (MidDate < new DateTime(2017, 8, 15))
            {
                switch (Number)
                {
                    case 1:
                        return $"April {MidDate.Year}";
                    case 2:
                        return $"Juli {MidDate.Year}";
                    case 3:
                        return $"Oktober {MidDate.Year}";
                    case 4:
                        return $"Februar {MidDate.Year + 1}";
                    default:
                        throw new IndexOutOfRangeException("Quarter was now 1, 2, 3, or 4. Real value: " + Number);
                }
            }
            switch (Number)
            {
                case 1:
                    return $"Maj {MidDate.Year}";
                case 2:
                    return $"August {MidDate.Year}";
                case 3:
                    return $"November {MidDate.Year}";
                case 4:
                    return $"Marts {MidDate.Year + 1}";
                default:
                    throw new IndexOutOfRangeException("Quarter was now 1, 2, 3, or 4. Real value: " + Number);
            }
        }
    }
}
