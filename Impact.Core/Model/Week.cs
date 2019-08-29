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
    public class Week : IClonable<Week>, IAbsorbable<Week>
    {
        public Week()
        {
            Dates = new SortedSet<DateTime>();
        }

        public int Number { get; set; }
        public ISet<DateTime> Dates { get; set; }
        public double TotalHours { get; set; }
        public decimal QuarterEdgeHours { get; set; }
        public decimal HolidayHours { get; set; }
        public decimal WorkHours { get; set; }
        public decimal InterestHours { get; set; }
        public decimal MovableOvertimeHours { get; set; }
        public decimal LockedOvertimeHours { get; set; }

        // So ugly please find a better solution. Really really
        public void CategorizeHours()
        {
            var hours = Math.Round(Convert.ToDecimal(TotalHours), 2);

            var workWeek = ApplicationConstants.NormalWorkWeek - HolidayHours - QuarterEdgeHours;
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
            foreach (var date in Dates)
            {
                if (date < quarter.From || quarter.To < date)
                    QuarterEdgeHours += ApplicationConstants.NormalWorkDay;
            }
        }
        
        public bool AbsorbHours(Week otherWeek, string propertyName)
        {
            var propertyInfo = otherWeek.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo == null) 
                return false;
            
            var movableHours = (decimal)propertyInfo.GetValue(otherWeek);

            var missingHours = ApplicationConstants.NormalWorkWeek - (WorkHours + HolidayHours + QuarterEdgeHours);
            if (missingHours > movableHours)
            {
                WorkHours += movableHours;
                propertyInfo.SetValue(otherWeek, 0m);
                return false;
            }

            WorkHours += missingHours;
            propertyInfo.SetValue(otherWeek, movableHours - missingHours);
            return true;
        }

        public object[] ToArray(decimal? defaultValue = 0)
        {
            return new object[]
            {
                GetDisplayNumber(),
                QuarterEdgeHours == 0 ? (decimal?)null : QuarterEdgeHours, "fill-color: #EFEFEF; opacity: 0.5",
                HolidayHours == 0 ? (decimal?)null : HolidayHours,
                WorkHours, // This needs to be animated, therefore must not be null
                InterestHours == 0 ? defaultValue : InterestHours, // This needs to be animated, therefore must not be null
                MovableOvertimeHours == 0 ? defaultValue : MovableOvertimeHours, // This needs to be animated, therefore must not be null
                LockedOvertimeHours == 0 ? (decimal?)null : LockedOvertimeHours
            };
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