using Base.Validations;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Threading.Tasks;

namespace Tests.UnitTests

{
    [TestClass]
    public class PasswordRulesTests
    {

        [TestMethod]
        public void GetErrorMessageForPasswordRules_EmptyPassword_ShouldReturnErrorMessage()
        {
            string password = "";
            var message = CheckPasswordRules.GetErrorMessageForPasswordRules(password);
            Assert.AreNotEqual(0, message.Length);
            Assert.AreEqual("password is empty", message);
        }

        [TestMethod]
        public void GetErrorMessageForPasswordRules_TooShortPassword_ShouldReturnErrorMessage()
        {
            string password = "1234567";
            var message = CheckPasswordRules.GetErrorMessageForPasswordRules(password);
            Assert.AreNotEqual(0, message.Length);
            Assert.AreEqual("password has a length of 7 chars (minimum 8)", message);
        }

        [TestMethod]
        public void GetErrorMessageForPasswordRules_CorrectPassword_ShouldReturnEmptyString()
        {
            string password = "4Bhif2021*";
            var message = CheckPasswordRules.GetErrorMessageForPasswordRules(password);
            Assert.AreEqual(0, message.Length);
        }

        [TestMethod]
        public void GetErrorMessageForPasswordRules_MissingDigits_ShouldReturnErrorMessage()
        {
            string password = "Bhifxxxk*";
            var message = CheckPasswordRules.GetErrorMessageForPasswordRules(password);
            Assert.AreNotEqual(0, message.Length);
            Assert.AreEqual("password must contain digits", message);
        }

        [TestMethod]
        public void GetErrorMessageForPasswordRules_MoreMissingParts_ShouldReturnErrorMessage()
        {
            string password = "1234abcd";
            var message = CheckPasswordRules.GetErrorMessageForPasswordRules(password);
            Assert.AreNotEqual(0, message.Length);
            Assert.AreEqual("password must contain upper letters, special chars", message);
        }

    }
}
