using System;
using System.Reflection;
using Impact.Core.Constants;
using Impact.Core.Interfaces;

namespace Impact.Core.Model
{
    public class Month : IClonable<Month>
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