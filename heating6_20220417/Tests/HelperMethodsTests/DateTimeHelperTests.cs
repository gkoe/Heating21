using System;

using Base.ExtensionMethods;
using Base.Helper;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.ExtensionMethodsTests
{
    [TestClass()]
    public class DateTimeHelperTests
    {
        [TestMethod()]
        public void ParseGermanDateTimeString_ValidDate_ShouldReturnCorrectDateTime()
        {
            var text = "1.1.2020";
            var actual = DateTimeHelpers.ParseGermanDateTimeString(text);
            Assert.AreEqual("01.01.2020 00:00:00", actual.ToString());
        }

        [TestMethod()]
        public void ParseGermanDateTimeString_ValidISODate_ShouldReturnCorrectDateTime()
        {
            var text = "2020-01-01";
            var actual = DateTimeHelpers.ParseGermanDateTimeString(text);
            Assert.AreEqual("01.01.2020 00:00:00", actual.ToString());
        }

        [ExpectedException(typeof(FormatException))]
        [TestMethod()]
        public void ParseGermanDateTimeString_MMDDYYYY_ShouldReturnCorrectDateTime()
        {
            var text = "09/12/2020";
            var actual = DateTimeHelpers.ParseGermanDateTimeString(text);
        }


        [TestMethod()]
        public void ParseGermanDateTimeString_ValidDateTime_ShouldReturnCorrectDateTime()
        {
            var text = "1.1.2020 14:13";
            var actual = DateTimeHelpers.ParseGermanDateTimeString(text);
            Assert.AreEqual("01.01.2020 14:13:00", actual.ToString());
        }

    }
}