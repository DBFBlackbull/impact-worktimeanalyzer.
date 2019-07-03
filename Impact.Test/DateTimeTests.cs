using System;
using NUnit.Framework;

namespace Impact.Test
{
    [TestFixture]
    public class DateTimeTests
    {
        [Test]
        public void OutOfBoundDay()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DateTime(2012, 4, 31));
        }   
    }
}