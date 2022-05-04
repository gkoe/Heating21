using System;

using Base.ExtensionMethods;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.ExtensionMethodsTests
{
    [TestClass()]
    public class DateTimeExtensionsTests
    {
        [TestMethod()]
        public void SetHoursAndMinutesStaticMethodTest()
        {
            DateTime dateTime = DateTime.Parse("1.12.2018");
            //dateTime.SetHoursAndMinutes(20, 30);
            dateTime = DateTimeExtensions.SetHoursAndMinutes(dateTime, 20, 30);
            // ReSharper disable once SpecifyACultureInStringConversionExplicitly
            Assert.AreEqual("01.12.2018 20:30:00", dateTime.ToString());
        }

        [TestMethod()]
        public void SetHoursAndMinutesExtensionMethodTest()
        {
            DateTime dateTime = DateTime.Parse("1.12.2018");
            dateTime = dateTime.SetHoursAndMinutes(20, 30);
            // ReSharper disable once SpecifyACultureInStringConversionExplicitly
            Assert.AreEqual("01.12.2018 20:30:00", dateTime.ToString());

        }
    }
}