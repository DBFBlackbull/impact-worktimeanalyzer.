using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Impact.Core.Model
{
    public class Week
    {
        public Week(string displayNumber)
        {
            DisplayNumber = displayNumber;
        }

        public string DisplayNumber { get; set; }
        public decimal? WorkHours { get; set; }
        public decimal? InterestHours { get; set; }
        public decimal? MoveableOvertimeHours { get; set; }
        public decimal? LockedOvertimeHours { get; set; }

        public object[] ToArray()
        {
            return new object[]
            {
                DisplayNumber,
                WorkHours,
                InterestHours,
                MoveableOvertimeHours,
                LockedOvertimeHours
            };
        }
    }
}
