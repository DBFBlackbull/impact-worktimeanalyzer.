using System;
using System.Globalization;
using Impact.Core.Constants;
using NUnit.Framework;

namespace Impact.Core.Test
{
    public class Tests
    {
        private static readonly CultureInfo DanishCultureInfo = new CultureInfo("da-DK");
        private static readonly Calendar DanishCalendar = DanishCultureInfo.Calendar;
        private static readonly DayOfWeek DanishFirstDayOfWeek = DanishCultureInfo.DateTimeFormat.FirstDayOfWeek;
        private static readonly CalendarWeekRule DanishCalendarWeekRule = DanishCultureInfo.DateTimeFormat.CalendarWeekRule;
        
        [SetUp]
        public void Setup()
        {
        }

        private static int GetDefaultWeekOfYear(DateTime dateTime)
        {
            return DanishCalendar.GetWeekOfYear(dateTime, DanishCalendarWeekRule, DanishFirstDayOfWeek);
        }

        private int GetModifiedWeekOfYear(DateTime dateTime)
        {
            return ApplicationConstants.GetWeekNumber(dateTime);
        }
        
        [Test]
        [TestCase(2016, 12, 26)]
        [TestCase(2016, 12, 27)]
        [TestCase(2016, 12, 28)]
        [TestCase(2016, 12, 29)]
        [TestCase(2016, 12, 30)]
        [TestCase(2016, 12, 31)]
        // Outlook and timelog rolls this backwards to week 52
        [TestCase(2017, 1, 1)]
        public void GetWeekNumber16_17(int year, int month, int day)
        {
            var dateTime = new DateTime(year, month, day, DanishCalendar);
            var weekOfYear = GetModifiedWeekOfYear(dateTime);
            Assert.AreEqual(52, weekOfYear);
        }
        
        [Test]
        // Outlook and timelog rolls this forwards to week 1
        [TestCase(2018, 12, 31)]
        [TestCase(2019, 1, 1)]
        [TestCase(2019, 1, 2)]
        [TestCase(2019, 1, 3)]
        [TestCase(2019, 1, 4)]
        [TestCase(2019, 1, 5)]
        [TestCase(2019, 1, 6)]
        public void GetWeekNumber18_19(int year, int month, int day)
        {
            var dateTime = new DateTime(year, month, day, DanishCalendar);
            var weekOfYear = GetModifiedWeekOfYear(dateTime);
            Assert.AreEqual(1, weekOfYear);
        }

        [Test]
        // Outlook and timelog rolls this forwards to week 1
        [TestCase(2019, 12, 30)]
        [TestCase(2019, 12, 31)]
        [TestCase(2020, 1, 1)]
        [TestCase(2020, 1, 2)]
        [TestCase(2020, 1, 3)]
        [TestCase(2020, 1, 4)]
        [TestCase(2020, 1, 5)]
        public void GetWeekNumber19_20(int year, int month, int day)
        {
            var dateTime = new DateTime(year, month, day, DanishCalendar);
            var weekOfYear = GetModifiedWeekOfYear(dateTime);
            Assert.AreEqual(1, weekOfYear);
        }

        [Test]
        [TestCase(2020, 12, 28)]
        [TestCase(2020, 12, 29)]
        [TestCase(2020, 12, 30)]
        [TestCase(2020, 12, 31)]
        // Outlook and timelog rolls these backwards to week 53
        [TestCase(2021, 1, 1)]
        [TestCase(2021, 1, 2)]
        [TestCase(2021, 1, 3)]
        public void GetWeekNumber20_21(int year, int month, int day)
        {
            var dateTime = new DateTime(year, month, day, DanishCalendar);
            var weekOfYear = GetModifiedWeekOfYear(dateTime);
            Assert.AreEqual(53, weekOfYear);
        }
        
        [Test]
        [TestCase(2021, 12, 27)]
        [TestCase(2021, 12, 28)]
        [TestCase(2021, 12, 29)]
        [TestCase(2021, 12, 30)]
        [TestCase(2021, 12, 31)]
        // Outlook and timelog rolls these backwards to week 52
        [TestCase(2022, 1, 1)]
        [TestCase(2022, 1, 2)]
        public void GetWeekNumber21_22(int year, int month, int day)
        {
            var dateTime = new DateTime(year, month, day, DanishCalendar);
            var weekOfYear = GetModifiedWeekOfYear(dateTime);
            Assert.AreEqual(52, weekOfYear);
        }
        
        [Test]
        // Outlook and timelog rolls these forwards to week 1
        [TestCase(2025, 12, 29)]
        [TestCase(2025, 12, 30)]
        [TestCase(2025, 12, 31)]
        [TestCase(2026, 1, 1)]
        [TestCase(2026, 1, 2)]
        [TestCase(2026, 1, 3)]
        [TestCase(2026, 1, 4)]
        public void GetWeekNumber25_26(int year, int month, int day)
        {
            var dateTime = new DateTime(year, month, day, DanishCalendar);
            var weekOfYear = GetModifiedWeekOfYear(dateTime);
            Assert.AreEqual(1, weekOfYear);
        }
    }
}