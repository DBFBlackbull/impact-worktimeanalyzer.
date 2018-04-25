using System;
using System.Reflection;
using Impact.Core.Contants;
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
        public double RegisteredHours { get; set; }
        public decimal Hours { get; set; }

        public void ConvertToDecimal()
        {
            Hours = Math.Round(Convert.ToDecimal(RegisteredHours), 2);
        }
        
        public bool AbsorbHours(Month otherMonth, string propertyName)
        {
            var moveableHours = otherMonth.Hours - ApplicationConstants.AwesomeThursdayApproximation;

            var missingHours = ApplicationConstants.AwesomeThursdayApproximation - Hours;
            if (missingHours > moveableHours)
            {
                Hours += moveableHours;
                otherMonth.Hours -= moveableHours;
                return false;
            }

            Hours += missingHours;
            otherMonth.Hours -= missingHours;
            return true;
        }
        
        public object[] ToArray()
        {
            return new object[]
            {
                $"{Date:Y}",
                Hours = Hours
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