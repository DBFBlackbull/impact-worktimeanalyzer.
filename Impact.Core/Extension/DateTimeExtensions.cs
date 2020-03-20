using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Impact.Core.Extension
{
    public static class DateTimeExtensions
    {
        public static DateTime BackTo(this DateTime dateTime, DayOfWeek dayOfWeek)
        {
            var newDateTime = dateTime;
            while (newDateTime.DayOfWeek != dayOfWeek)
            {
                newDateTime = newDateTime.AddDays(-1);
            }

            return newDateTime;
        }

        public static DateTime ForwardTo(this DateTime dateTime, DayOfWeek dayOfWeek)
        {
            var newDateTime = dateTime;
            while (newDateTime.DayOfWeek != dayOfWeek)
            {
                newDateTime = newDateTime.AddDays(1);
            }

            return newDateTime;
        }
    }
}
