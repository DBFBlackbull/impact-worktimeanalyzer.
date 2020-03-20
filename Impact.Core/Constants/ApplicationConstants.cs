using System;
using System.Collections.Generic;
using System.Globalization;

namespace Impact.Core.Constants
{
    public static class ApplicationConstants
    {
        public static readonly CultureInfo EnglishCultureInfo = new CultureInfo("en-US");
        public static readonly CultureInfo DanishCultureInfo = new CultureInfo("da-DK");
        private static readonly Calendar DanishCalendar = DanishCultureInfo.Calendar;
        private static readonly DayOfWeek DanishFirstDayOfWeek = DanishCultureInfo.DateTimeFormat.FirstDayOfWeek;
        private static readonly CalendarWeekRule DanishCalendarWeekRule = DanishCultureInfo.DateTimeFormat.CalendarWeekRule;
        public static int GetWeekNumber(DateTime date)
        {
            return DanishCalendar.GetWeekOfYear(date, DanishCalendarWeekRule, DanishFirstDayOfWeek);
        } 
        
        public static readonly DateTime TimelogStart = new DateTime(2012, 1, 1); // not used today, but handy for the future
        public static readonly DateTime RAndDStartDate = new DateTime(2018, 12, 11);
        public static readonly DateTime MiniVacationStart = new DateTime(2020, 5, 1);
        public static readonly DateTime MiniVacationEnd = new DateTime(2020, 8, 31);

        public const decimal NormalWorkDay = 7.5m;
        public const decimal NormalWorkWeek = 37.5m;
        public const decimal NormalWorkMonth = 162.5m;
        public const decimal InterestConst = 1.5m;
        public const decimal MovableConst = 5m;
        public const decimal AwesomeThursdayApproximation = NormalWorkDay / 2;

        public static class SessionName
        {
            public const string Token = "token";
            public const string Profile = "profile";
            public const string SelectedQuarter = "quarter";
        }
        
        public static class Color
        {
            public const string White = "#EFEFEF";
            public const string LightBlue = "#289eff";
            public const string Blue = "#3366cc";
            public const string Red = "#FF4635";
            public const string Orange = "orange";
            public const string Green = "green";
            public const string Black = "black";
        }
        
        public static string ImpactName = "Impact";
        // ReSharper disable StringLiteralTypo
        public static Dictionary<string, string> CustomerColors { get; } = new Dictionary<string, string>
        {
            {"nemlig.com", "#D4793A"},
            {"Helsam A/S", "#00AD4D"},
            {"Impact", "#FEFF00"},
        };
    }
}