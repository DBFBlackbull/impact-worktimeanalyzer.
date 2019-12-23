using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Impact.Core.Constants;
using Impact.Core.Interfaces;

namespace Impact.Core.Model
{
    public class Week : IClonable<Week>
    {
        public Week()
        {
            Dates = new SortedDictionary<DateTime, decimal>();
        }

        public int Number { get; set; }
        public SortedDictionary<DateTime, decimal> Dates { get; set; }
        public decimal TotalHours { get; set; }
        public decimal NormalWorkWeek { get; set; }
        public decimal QuarterEdgeHours { get; set; }
        public decimal HolidayHours { get; set; }
        public decimal WorkHours { get; set; }
        public decimal InterestHours { get; set; }
        public decimal MovableOvertimeHours { get; set; }
        public decimal LockedOvertimeHours { get; set; }

        // So ugly please find a better solution. Really really
        public void CategorizeHours()
        {
            var hours = Math.Round(TotalHours, 2);

            var workWeek = NormalWorkWeek - HolidayHours - QuarterEdgeHours;
            if (hours >= workWeek)
            {
                WorkHours = workWeek;
                hours -= workWeek;
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

            if (hours >= ApplicationConstants.MovableConst)
            {
                MovableOvertimeHours = ApplicationConstants.MovableConst;
                hours -= ApplicationConstants.MovableConst;
            }
            else
            {
                MovableOvertimeHours = hours;
                return;
            }

            LockedOvertimeHours = hours;
        }

        public void AddQuarterEdgeHours(Quarter quarter)
        {
            foreach (var kvp in Dates)
            {
                var date = kvp.Key;
                if (ApplicationConstants.GetWeekNumber(date) == 53)
                    continue;
                
                if (date < quarter.From || quarter.To < date)
                    QuarterEdgeHours += kvp.Value;
            }
        }
        
        public object?[] ToArray(bool manyWorkWeeks, decimal? defaultValue = 0)
        {
            var objects = new List<object?>
            {
                GetDisplayNumber(),
                QuarterEdgeHours == 0 ? (decimal?)null : QuarterEdgeHours, "fill-color: #EFEFEF; opacity: 0.5",
                HolidayHours == 0 ? (decimal?)null : HolidayHours,
                WorkHours, // This needs to be animated, therefore must not be null
                InterestHours == 0 ? defaultValue : InterestHours, // This needs to be animated, therefore must not be null
                MovableOvertimeHours == 0 ? defaultValue : MovableOvertimeHours, // This needs to be animated, therefore must not be null
                LockedOvertimeHours == 0 ? (decimal?)null : LockedOvertimeHours
            };
            if (manyWorkWeeks)
                objects.Insert(1, NormalWorkWeek);
            
            return objects.ToArray();
        }

        public string GetDisplayNumber()
        {
            return $"Uge {Number}";
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
}