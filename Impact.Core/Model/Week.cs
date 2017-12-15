using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Impact.Core.Contants;

namespace Impact.Core.Model
{
    public class Week : IClonable<Week>
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

        // So ugly please find a better solution. Really really
        public void CategorizeHours()
        {
            var hours = Math.Round(Convert.ToDecimal(TotalHours), 2);

            var workHoursMinusHolidayHours = ApplicationConstants.NormalWorkWeek - HolidayHours;
            if (hours >= workHoursMinusHolidayHours)
            {
                WorkHours = workHoursMinusHolidayHours;
                hours -= workHoursMinusHolidayHours;
            }
            else
            {
                WorkHours = hours;
                return;
            }

            if (hours >= ApplicationConstants.InterestConst)
            {
                InterestHours = ApplicationConstants.InterestConst;
                hours -= ApplicationConstants.InterestConst;
            }
            else
            {
                InterestHours = hours;
                return;
            }

            if (hours >= ApplicationConstants.MoveableConst)
            {
                MoveableOvertimeHours = ApplicationConstants.MoveableConst;
                hours -= ApplicationConstants.MoveableConst;
            }
            else
            {
                MoveableOvertimeHours = hours;
                return;
            }

            LockedOvertimeHours = hours;

            return;
        }
        
        public bool AbsorbHours(Week otherWeek, string propertyName)
        {
            var propertyInfo = otherWeek.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo == null) 
                return false;
            
            var moveableHours = (decimal)propertyInfo.GetValue(otherWeek);

            var missingHours = ApplicationConstants.NormalWorkWeek - (WorkHours + HolidayHours);
            if (missingHours > moveableHours)
            {
                WorkHours += moveableHours;
                propertyInfo.SetValue(otherWeek, 0m);
                return false;
            }

            WorkHours += missingHours;
            propertyInfo.SetValue(otherWeek, moveableHours - missingHours);
            return true;
        }

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

        public Week Clone()
        {
            var week = new Week();
            foreach (var property in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = property.GetValue(this);
                property.SetValue(week, value);
            }

            return week;
        }
    }

    public interface IClonable<T>
    {
        T Clone();
    }
}