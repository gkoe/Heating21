
using Base.Helper;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

namespace Tests.UnitTests
{
    [TestClass]
    public class JwtParserTests
    {
        readonly string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoiYWRtaW5AZ21haWwuY29tIiwiSWQiOiIwNTM0ZGZiYi0yZGYyLTQ4MDUtOGM4OC0xZmExOTljODVhYjQiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsImV4cCI6MTYyMzMzMTU2NiwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NTAwMS8iLCJhdWQiOiJodHRwczovL2xvY2FsaG9zdDo1MDAxLyJ9.ZNYOsl1F7qLO2_Ahvj9dkfyTUPmK7VJyz4lknB_6ZQM";

        [TestMethod]
        public void JwtParser_GetClaims_ShouldReturn7Claims()
        {
            var claims = JwtParser.ParseClaimsFromJwt(token);
            Assert.AreEqual(7, claims.Count());
        }

        [TestMethod]
        public void JwtParser_GetRoles_ShouldReturn1Role()
        {
            var roles = JwtParser.ParseRolesFromJwt(token);
            Assert.AreEqual(1, roles.Length);
        }

        [TestMethod]
        public void JwtParser_GetExpirationTime_ShouldReturnCorrectValue()
        {
            var expirationTime = JwtParser.ParseExpirationTimeFromJwt(token);
            Assert.AreEqual("10.06.2021 15:26:06", expirationTime.ToString());
        }

        [TestMethod]
        public void JwtParser_GetMail_ShouldReturnCorrectMail()
        {
            var mail = JwtParser.ParseMailFromJwt(token);
            Assert.AreEqual("admin@gmail.com", mail);
        }

        [TestMethod]
        public void JwtParser_GetId_ShouldReturnCorrectId()
        {
            var mail = JwtParser.ParseIdFromJwt(token);
            Assert.AreEqual("0534dfbb-2df2-4805-8c88-1fa199c85ab4", mail);
        }
    }
}
