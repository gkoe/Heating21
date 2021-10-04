using System;

using Base.Helper;
using Base.Helper.ExtensionMethods;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.ExtensionMethodsTests
{
    [TestClass()]
    public class StringExtensionsTests
    {
        [TestMethod()]
        public void Shorten_EmptyString_ShouldReturnEmptyString()
        {
            string text = string.Empty;
            string actual = text.Shorten(10);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod()]
        public void Shorten_NullString_ShouldReturnNullString()
        {
            string actual = StringExtensions.Shorten(null, 10);
            Assert.AreEqual(null, actual);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Shorten_MaxLenghtTooShort_ShouldThrowException()
        {
            string text = "Hallo";
            text.Shorten(2);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Shorten_MaxLenghtNegative_ShouldThrowException()
        {
            string text = "Hallo";
            text.Shorten(-2);
        }

        [TestMethod()]
        public void Shorten_MaxLenghtEqual3_ShouldReturnThreeDots()
        {
            string text = "Hallo";
            string actual = text.Shorten(3);
            Assert.AreEqual("...", actual);
        }

        [TestMethod()]
        public void Shorten_MaxLenghtEqualTextLength_ShouldReturnOriginText()
        {
            string text = "Hallo";
            string actual = text.Shorten(5);
            Assert.AreEqual(text, actual);
        }

        [TestMethod()]
        public void Shorten_MaxLenghtGreaterTextLength_ShouldReturnOriginText()
        {
            string text = "Hallo";
            string actual = text.Shorten(10);
            Assert.AreEqual(text, actual);
        }

        [TestMethod()]
        public void Shorten_MaxLenghtLowerTextLength_ShouldReturnOriginText()
        {
            string text = "Hallo";
            string actual = text.Shorten(4);
            Assert.AreEqual("H...", actual);
        }

        [TestMethod()]
        public void Shorten_MaxLengthFix10_ShouldReturnShortenedText()
        {
            string text = "Hallo jfhksjfhhsfdö 4bhif";
            string actual = text.Shorten();
            Assert.AreEqual("Hallo j...", actual);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TryParseToDouble_Null_ShouldThrowException()
        {
            string text = null;
            _ = text.TryParseToDouble();
        }

        [TestMethod()]
        public void TryParseToDouble_NumberWithComma_ShouldReturnValue()
        {
            string text = "3,14";
            var actual = text.TryParseToDouble();
            Assert.AreEqual(3.14, actual);
        }

        [TestMethod()]
        public void TryParseToDouble_NumberWithDot_ShouldReturnValue()
        {
            string text = "3.14";
            var actual = text.TryParseToDouble();
            Assert.AreEqual(3.14, actual);
        }

        [TestMethod()]
        public void TryParseToDouble_NumberWithDotAtEnd_ShouldReturnValue()
        {
            string text = "3.";
            var actual = text.TryParseToDouble();
            Assert.AreEqual(3, actual);
        }

        [TestMethod()]
        public void TryParseToDouble_NumberWith5Decimals_ShouldReturnValue()
        {
            string text = "3.12345";
            var actual = text.TryParseToDouble();
            Assert.AreEqual(3.12345, actual);
        }

        [TestMethod()]
        public void TryParseToDouble_ThousendComma_ShouldReturnValue()
        {
            string text = "3,333.33";
            var actual = text.TryParseToDouble();
            Assert.AreEqual(3333.33, actual);
        }

        [TestMethod()]
        public void TryParseToDouble_TwoThousendComma_ShouldReturnValue()
        {
            string text = "3,333,333.33";
            var actual = text.TryParseToDouble();
            Assert.AreEqual(3333333.33, actual);
        }

        [TestMethod()]
        public void TryParseToDouble_ThousendDot_ShouldReturnValue()
        {
            string text = "3.333,33";
            var actual = text.TryParseToDouble();
            Assert.AreEqual(3333.33, actual);
        }

        [TestMethod()]
        public void TryParseToDouble_twoThousendDot_ShouldReturnValue()
        {
            string text = "3.333.333,33";
            var actual = text.TryParseToDouble();
            Assert.AreEqual(3333333.33, actual);
        }

        [TestMethod()]
        public void TryParseToDouble_NoNumber_ShouldReturnNull()
        {
            string text = "Hallo.,tt";
            var actual = text.TryParseToDouble();
            Assert.IsNull(actual);
        }

        [TestMethod()]
        public void TryParseToDouble_EmptyString_ShouldReturnNull()
        {
            string text = string.Empty;
            var actual = text.TryParseToDouble();
            Assert.IsNull(actual);
        }

        [TestMethod()]
        public void TryParseToDouble_OnlySeparator_ShouldReturnNull()
        {
            string text = ",";
            var actual = text.TryParseToDouble();
            Assert.IsNull(actual);
        }

        [TestMethod()]
        public void TryParseToDouble_OnlySeparators_ShouldReturnNull()
        {
            string text = ".,.";
            var actual = text.TryParseToDouble();
            Assert.IsNull(actual);
        }

        [TestMethod()]
        public void Reverse_EmptyString_ShouldReturnEmptyString()
        {
            string text = string.Empty;
            string expected = "";
            var actual = text.Reverse();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Reverse_Null_ShouldThrowException()
        {
            string text = null;
            _ = text.Reverse();
        }


        [TestMethod()]
        public void Reverse_String_ShouldReturnReversedString()
        {
            string text = "12345";
            string expected = "54321";
            var actual = text.Reverse();
            Assert.AreEqual(expected, actual);
        }


    }
}