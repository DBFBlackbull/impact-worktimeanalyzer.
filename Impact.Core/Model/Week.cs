using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Impact.Core.Model
{
    public class Week
    {
        public Week()
        {
            Dates = new SortedSet<DateTime>();
        }

        public int Number { get; set; }
        public ISet<DateTime> Dates { get; set; }
        public double TotalHours { get; set; }
        public decimal HolidayHours { get; set; }
        public decimal WorkHours { get; set; }
        public decimal InterestHours { get; set; }
        public decimal MoveableOvertimeHours { get; set; }
        public decimal LockedOvertimeHours { get; set; }

        public object[] ToArray()
        {
            return new object[]
            {
                "Uge " + Number,
                HolidayHours,
                WorkHours,
                InterestHours,
                MoveableOvertimeHours,
                LockedOvertimeHours
            };
        }
    }
}
