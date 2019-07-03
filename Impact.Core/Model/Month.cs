using System;
using System.Reflection;
using Impact.Core.Constants;
using Impact.Core.Interfaces;

namespace Impact.Core.Model
{
    public class Month : IClonable<Month>, IAbsorbable<Month>
    {
        private Month()
        {
        }

        public Month(DateTime date)
        {
            Date = date;
        }

        public DateTime Date { get; set; }
        public double AwesomeThursdayRawHours { get; set; }
        public double RAndDRawHours { get; set; }
        public decimal AwesomeThursdayHours { get; set; }
        public decimal RAndDHours { get; set; }

        public void ConvertToDecimal()
        {
            AwesomeThursdayHours = Math.Round(Convert.ToDecimal(AwesomeThursdayRawHours), 2);
            RAndDHours = Math.Round(Convert.ToDecimal(RAndDRawHours), 2);
        }
        
        public bool AbsorbHours(Month otherMonth, string propertyName)
        {
            if (Date < ApplicationConstants.RAndDStartDate)
            {
                var movableAwesomeHours = otherMonth.AwesomeThursdayHours - ApplicationConstants.AwesomeThursdayApproximation;
                var missingAwesomeHours = ApplicationConstants.AwesomeThursdayApproximation - AwesomeThursdayHours;
                
                if (missingAwesomeHours > movableAwesomeHours)
                {
                    AwesomeThursdayHours += movableAwesomeHours;
                    otherMonth.AwesomeThursdayHours -= movableAwesomeHours;
                    return false;
                }
                
                AwesomeThursdayHours += missingAwesomeHours;
                otherMonth.AwesomeThursdayHours -= missingAwesomeHours;
                return true;
            }
            
            var movableHours = (otherMonth.AwesomeThursdayHours + otherMonth.RAndDHours) - ApplicationConstants.AwesomeThursdayApproximation;
            var missingHours = ApplicationConstants.AwesomeThursdayApproximation - (AwesomeThursdayHours + RAndDHours);
            
            if (missingHours > movableHours)
            {
                RAndDHours += movableHours;
                otherMonth.RAndDHours -= movableHours;
                return false;
            }

            RAndDHours += missingHours;
            otherMonth.RAndDHours -= missingHours;
            return true;
        }
        
        public object[] ToArray()
        {
            return new object[]
            {
                $"{Date:Y}",
                AwesomeThursdayHours = AwesomeThursdayHours,
                RAndDHours = RAndDHours,
            };
        }
        
        public Month Clone()
        {
            var month = new Month();
            foreach (var property in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = property.GetValue(this);
                property.SetValue(month, value);
            }

            return month;
        }
    }
}