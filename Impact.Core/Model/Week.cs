﻿using System;
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
        public decimal QuarterEdgeHours { get; set; }
        public decimal HolidayHours { get; set; }
        public decimal WorkHours { get; set; }
        public decimal InterestHours { get; set; }
        public decimal MoveableOvertimeHours { get; set; }
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

        public void AddQuarterEdgeHours(Quarter quarter)
        {
            foreach (var date in Dates)
            {
                if (date < quarter.From || date > quarter.To)
                    QuarterEdgeHours += ApplicationConstants.NormalWorkDay;
            }
        }
        
        public bool AbsorbHours(Week otherWeek, string propertyName)
        {
            var propertyInfo = otherWeek.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo == null) 
                return false;
            
            var moveableHours = (decimal)propertyInfo.GetValue(otherWeek);

            var missingHours = ApplicationConstants.NormalWorkWeek - (WorkHours + HolidayHours + QuarterEdgeHours);
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
                QuarterEdgeHours == 0 ? (decimal?)null : QuarterEdgeHours, "fill-color: #EFEFEF; opacity: 0.5",
                HolidayHours == 0 ? (decimal?)null : HolidayHours,
                WorkHours == 0 ? (decimal?)null : WorkHours,
                InterestHours == 0 ? (decimal?)null : InterestHours,
                MoveableOvertimeHours == 0 ? (decimal?)null : MoveableOvertimeHours,
                LockedOvertimeHours == 0 ? (decimal?)null : LockedOvertimeHours
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