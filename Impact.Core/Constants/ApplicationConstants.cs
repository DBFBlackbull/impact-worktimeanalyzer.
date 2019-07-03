using System;
using System.Globalization;

namespace Impact.Core.Constants
{
    public static class ApplicationConstants
    {
        public static readonly CultureInfo DanishCultureInfo = new CultureInfo("da-DK");
        public static readonly DateTime TimelogStart = new DateTime(2012, 1, 1);
        public static readonly DateTime RAndDStartDate = new DateTime(2018, 12, 11);

        public const decimal NormalWorkDay = 7.5m;
        public const decimal NormalWorkWeek = 37.5m;
        public const decimal InterestConst = 1.5m;
        public const decimal MovableConst = 5m;
        public const decimal AwesomeThursdayApproximation = NormalWorkDay / 2;

        public const string Token = "token";
        public const string FullName = "fullName";
        public const int PageSize = 500;
        
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
    }
}