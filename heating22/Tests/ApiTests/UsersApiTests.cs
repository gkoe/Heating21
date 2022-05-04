using Api;
using Api.Controllers.Auth;

using Base.DataTransferObjects;
using Base.Helper;

using Core.Contracts;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using Persistence;

using Serilog;

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Tests.ApiTests;

namespace Tests.IntegrationTests
{
    [TestClass]
    public class UsersApiTests
    {
        const string BaseUrl = "/api/users";

        [TestInitialize]
        public async Task Setup()
        {
            await ApiTestHelpers.RecreateDatabaseAsync();
        }

        [TestCleanup]
        public void TearDown()
        {
            // Runs after each test. (Optional)
        }

        private static async Task<UserGetDto[]> GetUsersAsync()
        {
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var response = await client.GetAsync($"{BaseUrl}/getall");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<UserGetDto[]>(responseString);
            return users ?? Array.Empty<UserGetDto>();
        }



        [TestMethod]
        public async Task GetAll_Authorized_ShouldReturn2Users()
        {
            // Arrange
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            // Act
            var response = await client.GetAsync($"{BaseUrl}/getall");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<UserGetDto[]>(responseString);
            // Assert
            Assert.AreEqual(2, result?.Length);
        }

        [TestMethod]
        public async Task GetAll_UnAuthorized_ShouldReturnUnAuthorized()
        {
            // Arrange
            var client = ApiTestHelpers.GetClient();
            // Act
            var response = await client.GetAsync($"{BaseUrl}/getall");
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }


        [TestMethod]
        public async Task Update_NotLoggedIn_ShouldReturnUnAuthorized()
        {
            // Arrange
            var users = await GetUsersAsync();
            var user = users.Single(u => u.Email == "user@htl.at");
            var client = ApiTestHelpers.GetClient();
            var updateUser = new UserPutDto
            {
                Id = user.Id,
                Email = user.Email,
                RoleName = user.RoleName
            };
            var content = JsonConvert.SerializeObject(updateUser);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PutAsync($"{BaseUrl}/update", bodyContent);
            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task Update_WrongCredentials_ShouldReturnBadRequest()
        {
            // Arrange
            var users = await GetUsersAsync();
            var user = users.Single(u => u.Email == "user@htl.at");
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var putDto = new UserPutDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = "Benutzer",
                RoleName = user.RoleName
            };
            var content = JsonConvert.SerializeObject(putDto);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PutAsync($"{BaseUrl}/update", bodyContent);
            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            content = await response.Content.ReadAsStringAsync();
            Assert.AreEqual("you are not allowed to update another user", content);
        }

        [TestMethod]
        public async Task Update_NewPasswordWrong_ShouldReturnBadRequest()
        {
            // Arrange
            var users = await GetUsersAsync();
            var admin = users.Single(u => u.Email == "admin@htl.at");
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var putDto = new UserPutDto
            {
                Id = admin.Id,
                Email = admin.Email,
                Name = admin.Name,
                RoleName = admin.RoleName,
                OldPassword = "Admin123*",
                NewPassword = "XXX"  // Keine Ziffern, kein Sonderzeichen, zu kurz
            };
            var content = JsonConvert.SerializeObject(putDto);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PutAsync($"{BaseUrl}/update", bodyContent);
            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            content = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(content.Contains("password has a length of 3 chars"));
        }

        [TestMethod]
        public async Task Update_NewPasswordOk_ShouldReturnOk()
        {
            // Arrange
            var users = await GetUsersAsync();
            var admin = users.Single(u => u.Email == "admin@htl.at");
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var putDto = new UserPutDto
            {
                Id = admin.Id,
                Email = admin.Email,
                Name = admin.Name,
                RoleName = admin.RoleName,
                OldPassword = "Admin123*",
                NewPassword = "NewPw123*"  // OK
            };
            var content = JsonConvert.SerializeObject(putDto);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PutAsync($"{BaseUrl}/update", bodyContent);
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task UpsertByAdmin_NewPasswordOk_ShouldReturnOk()
        {
            // Arrange
            var users = await GetUsersAsync();
            var user = users.Single(u => u.Email == "user@htl.at");
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var putDto = new UserPutDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                RoleName = user.RoleName,
                NewPassword = "NewPw123*"  // OK
            };
            var content = JsonConvert.SerializeObject(putDto);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PutAsync($"{BaseUrl}/upsertbyadmin", bodyContent);
            // Assert
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            try  // mit neuem Kennwort anmelden
            {
                var userClient = await ApiTestHelpers.GetAuthorizedClientAsync(new LoginRequestDto(putDto.Email, putDto.NewPassword));
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task UpsertByAdmin_NewPasswordNotOk_ShouldReturnBadRequest()
        {
            // Arrange
            var users = await GetUsersAsync();
            var user = users.Single(u => u.Email == "user@htl.at");
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var putDto = new UserPutDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                RoleName = user.RoleName,
                NewPassword = "NewPw123"  // kein Somnderzeichen
            };
            var content = JsonConvert.SerializeObject(putDto);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PutAsync($"{BaseUrl}/upsertbyadmin", bodyContent);
            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [TestMethod]
        public async Task UpsertByAdmin_NewRoleOk_ShouldReturnOk()
        {
            // Arrange
            var users = await GetUsersAsync();
            var user = users.Single(u => u.Email == "user@htl.at");
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var putDto = new UserPutDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                RoleName = MagicStrings.Role_Admin,
            };
            var content = JsonConvert.SerializeObject(putDto);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PutAsync($"{BaseUrl}/upsertbyadmin", bodyContent);
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<UserGetDto>(responseString);
            Assert.IsNotNull(result);
            Assert.AreEqual(MagicStrings.Role_Admin, result.RoleName);  
        }

        [TestMethod]
        public async Task UpsertByAdmin_NewRoleNotOk_ShouldReturnBadRequest()
        {
            // Arrange
            var users = await GetUsersAsync();
            var user = users.Single(u => u.Email == "user@htl.at");
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var putDto = new UserPutDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                RoleName = "UnKnown",
            };
            var content = JsonConvert.SerializeObject(putDto);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PutAsync($"{BaseUrl}/upsertbyadmin", bodyContent);
            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Login_Ok_ShouldReturnLoginPostDto()
        {
            // Arrange
            var users = await GetUsersAsync();
            var user = users.Single(u => u.Email == "user@htl.at");
            var client = ApiTestHelpers.GetClient();
            var loginDto = new LoginRequestDto
            {
                Email = "user@htl.at",
                Password = "User123*"
            };
            var content = JsonConvert.SerializeObject(loginDto);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PostAsync($"{BaseUrl}/login", bodyContent);
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<LoginResponseDto>(responseString);
            Assert.IsNotNull(result);
            Assert.AreEqual("user@htl.at", result.Email);
            var userDetails = JwtParser.ParseUserDetails(result.AuthToken);
            Assert.IsNotNull(userDetails);
            Assert.AreEqual("user@htl.at", userDetails.Email);
        }

        [TestMethod]
        public async Task Login_WrongEmail_ShouldReturnUnauthorized()
        {
            // Arrange
            var client = ApiTestHelpers.GetClient();
            var loginDto = new LoginRequestDto
            {
                Email = "xxx@htl.at",
                Password = "User123*"
            };
            var content = JsonConvert.SerializeObject(loginDto);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PostAsync($"{BaseUrl}/login", bodyContent);
            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.AreEqual("Invalid Authentication", responseString);
        }

        [TestMethod]
        public async Task Login_WrongPassword_ShouldReturnUnauthorized()
        {
            // Arrange
            var client = ApiTestHelpers.GetClient();
            var loginDto = new LoginRequestDto
            {
                Email = "user@htl.at",
                Password = "User123-"
            };
            var content = JsonConvert.SerializeObject(loginDto);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            // Act
            var response = await client.PostAsync($"{BaseUrl}/login", bodyContent);
            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.AreEqual("Invalid Authentication", responseString);
        }



        [TestMethod]
        public async Task Logout_NotLoggedIn_ShouldReturnUnauthorized()
        {
            var users = await GetUsersAsync();
            var admin = users.Single(u => u.Email == "admin@htl.at");
            var client = ApiTestHelpers.GetClient();
            var resp = await client.GetAsync($"{BaseUrl}/logout/{admin.Id}");
            Assert.AreEqual(resp.StatusCode, HttpStatusCode.Unauthorized);
        }


        [TestMethod]
        public async Task Logout_LoggedIn_ShoudlOk()
        {
            // Arrange
            var users = await GetUsersAsync();
            var admin = users.Single(u => u.Email == "admin@htl.at");
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            // Act
            var resp = await client.GetAsync($"{BaseUrl}/logout/{admin.Id}");
            Assert.AreEqual(resp.StatusCode, HttpStatusCode.OK);
        }

        //[TestMethod]
        //public async Task Register_Correct_ShouldReturnAddedAuthUser()
        //{
        //    var email = "newuser@htl.at";
        //    var newUser = new AuthUserPostDto(email, "Test123*");
        //    var content = JsonConvert.SerializeObject(newUser);
        //    var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
        //    // Act
        //    var response = await _client.PostAsync($"{BaseUrl}/Register", bodyContent);
        //    var responseContent = await response.Content.ReadAsStringAsync();
        //    var result = JsonConvert.DeserializeObject<AuthUserGetDto>(responseContent);
        //    // Assert
        //    Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        //    Assert.AreEqual(2, result.Id);
        //    Assert.AreEqual(email, result.Email);
        //}


        //[TestMethod]
        //public async Task Register_InvalidEmail_ShouldReturnBadRequest()
        //{
        //    var email = "tet@";
        //    var newUser = new AuthUserPostDto(email, "Test123*");
        //    var content = JsonConvert.SerializeObject(newUser);
        //    var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
        //    var response = await _client.PostAsync($"{BaseUrl}/Register", bodyContent);
        //    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        //    var responseContent = await response.Content.ReadAsStringAsync();
        //    Assert.IsTrue(responseContent.Contains("The Email field is not a valid e-mail address."));
        //}

        //[TestMethod]
        //public async Task Register_InvalidPassword_ShouldReturnBadRequest()
        //{
        //    var email = "tet@hello.org";
        //    var newUser = new AuthUserPostDto(email, "Test1234");
        //    var content = JsonConvert.SerializeObject(newUser);
        //    var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
        //    var response = await _client.PostAsync($"{BaseUrl}/Register", bodyContent);
        //    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        //    var responseContent = await response.Content.ReadAsStringAsync();
        //    Assert.IsTrue(responseContent.Contains("Password needs to have one special char"));
        //}

        //[TestMethod]
        //public async Task Register_EmailExists_ShouldReturnBadRequest()
        //{
        //    var email = "admin@htl.at";
        //    var newUser = new AuthUserPostDto(email, "Test123&");
        //    var content = JsonConvert.SerializeObject(newUser);
        //    var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
        //    var response = await _client.PostAsync($"{BaseUrl}/Register", bodyContent);
        //    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        //    var responseContent = await response.Content.ReadAsStringAsync();
        //    Assert.IsTrue(responseContent.Contains($"The duplicate key value is ({email})."));
        //}

    }
}
