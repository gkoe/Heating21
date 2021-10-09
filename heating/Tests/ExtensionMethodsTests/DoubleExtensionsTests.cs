using System;

using Base.ExtensionMethods;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.ExtensionMethodsTests
{
    [TestClass()]
    public class DoubleExtensionsTests
    {
        [TestMethod()]
        public void ToLegalDouble_SimplePositiveNumber_ShouldReturnSameDouble()
        {
            double number = 12.3456;
            double actual = number.ToLegalDouble();
            Assert.AreEqual(number, actual);
        }

        [TestMethod()]
        public void ToLegalDouble_SimpleNegativeNumber_ShouldReturnSameDouble()
        {
            double number = -12.3456;
            double actual = number.ToLegalDouble();
            Assert.AreEqual(number, actual);
        }

        [TestMethod()]
        public void ToLegalDouble_PositiveInfinity_ShouldReturnMaxValue()
        {
            double number = double.PositiveInfinity;
            double actual = number.ToLegalDouble();
            Assert.AreEqual(double.MaxValue, actual);
        }

        [TestMethod()]
        public void ToLegalDouble_NegativeInfinity_ShouldReturnMinValue()
        {
            double number = double.NegativeInfinity;
            double actual = number.ToLegalDouble();
            Assert.AreEqual(double.MinValue, actual);
        }

        [TestMethod()]
        public void ToLegalDouble_NotANumber_ShouldReturnMinValue()
        {
            double number = double.NaN;
            double actual = number.ToLegalDouble();
            Assert.AreEqual(double.MinValue, actual);
        }

        [TestMethod()]
        public void ToGermanString_SimplePositiveNumber_ShouldReturnWith2Decimals()
        {
            double number = 12.3456;
            string actual = number.ToGermanString();
            Assert.AreEqual("12,35", actual);
        }

        [TestMethod()]
        public void ToGermanString_SimpleNegativeNumber_ShouldReturnWith2Decimals()
        {
            double number = -12.3456;
            string actual = number.ToGermanString();
            Assert.AreEqual("-12,35", actual);
        }

        [TestMethod()]
        public void ToGermanString_SimpleNegativeNumber_ShouldReturnWith3Decimals()
        {
            double number = 12.3456;
            string actual = number.ToGermanString(3);
            Assert.AreEqual("12,346", actual);
        }

        [TestMethod()]
        public void ToGermanString_SimpleNegativeNumber_ShouldReturnWithoutDecimals()
        {
            double number = 12.3456;
            string actual = number.ToGermanString(0);
            Assert.AreEqual("12", actual);
        }

    }
}