
using Api;
using Api.Controllers.Auth;

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
using System.Text;
using System.Threading.Tasks;

using Tests.ApiTests;

namespace Tests.IntegrationTests
{
    [TestClass]
    public class RolesApiTests
    {
        const string BaseUrl = "/api/roles";

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

        [TestMethod]
        public async Task Get_All_ShouldReturn3RolesFromInitialization()
        {
            // Act
            var result = await GetRolesAsync();
            // Assert
            Assert.AreEqual(3, result?.Length);
        }

        private static async Task<RoleGetDto[]> GetRolesAsync()
        {
            var client = ApiTestHelpers.GetClient();    
            var response = await client.GetAsync($"{BaseUrl}/get");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var roles = JsonConvert.DeserializeObject<RoleGetDto[]>(responseString);
            return roles ?? Array.Empty<RoleGetDto>();
        }

        [TestMethod]
        public async Task GetById_AdminId_ShouldReturnAdmin()
        {
            // Arrange
            var roles = await GetRolesAsync();
            var adminRole = roles.Single(r => r.Name == "Admin");
            // Act
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var response = await client.GetAsync($"{BaseUrl}/getbyid/{adminRole.Id}");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoleGetDto>(responseString);
            // Assert
            Assert.AreEqual("Admin", result?.Name);
        }

        [TestMethod]
        public async Task GetById_IllegalId_ShouldReturnNotFound()
        {
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var response = await client.GetAsync($"{BaseUrl}/getbyid/123435566");
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task AddAuthRole_Correct_ShouldReturnAddedAuthRole()
        {
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var name = "newRole";
            var newRole = new RolePostDto(name);
            var content = JsonConvert.SerializeObject(newRole);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{BaseUrl}/post", bodyContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoleGetDto>(responseContent);
            Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode);
            Assert.IsNotNull(result);
            Assert.AreEqual(name, result.Name);
        }

        [TestMethod]
        public async Task AddAuthRole_ExistingRole_ShouldReturnBadRequest()
        {
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var name = "Admin";
            var newRole = new RolePostDto(name);
            var content = JsonConvert.SerializeObject(newRole);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{BaseUrl}/post", bodyContent);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.AreEqual("Role Admin already exists!", responseContent);
        }

        [TestMethod]
        public async Task AddAuthRole_EmptyRole_ShouldReturnBadRequest()
        {
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var name = "";
            var newRole = new RolePostDto(name);
            var content = JsonConvert.SerializeObject(newRole);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{BaseUrl}/post", bodyContent);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseContent.Contains("The Name field is required"));
        }

        [TestMethod]
        public async Task AddAuthRole_Unauthorized_ShouldReturnUnauthorized()
        {
            var client = ApiTestHelpers.GetClient();
            var name = "";
            var newRole = new RolePostDto(name);
            var content = JsonConvert.SerializeObject(newRole);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{BaseUrl}/post", bodyContent);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }


        [TestMethod]
        public async Task UpdateAuthRole_Unauthorized_ShouldReturnUnauthorized()
        {
            // Arrange
            var roles = await GetRolesAsync();
            var adminRole = roles.Single(r => r.Name == "Admin");
            var client = ApiTestHelpers.GetClient();
            var putRole = new RolePutDto(adminRole.Id, "Administrator");
            var putContent = JsonConvert.SerializeObject(putRole);
            var bodyContent = new StringContent(putContent, Encoding.UTF8, "application/json");
            // Act
            var putResponse = await client.PutAsync($"{BaseUrl}/put/{putRole.Id}", bodyContent);
            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, putResponse.StatusCode);
        }

        [TestMethod]
        public async Task UpdateAuthRole_EmptyRole_ShouldReturnBadRequest()
        {
            // Arrange
            var roles = await GetRolesAsync();
            var adminRole = roles.Single(r => r.Name == "Admin");
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var putRole = new RolePutDto(adminRole.Id, "");
            var putContent = JsonConvert.SerializeObject(putRole);
            var bodyContent = new StringContent(putContent, Encoding.UTF8, "application/json");
            // Act
            var putResponse = await client.PutAsync($"{BaseUrl}/put/{putRole.Id}", bodyContent);
            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, putResponse.StatusCode);
            var responseContent = await putResponse.Content.ReadAsStringAsync();
            Assert.IsTrue(responseContent.Contains("The Name field is required"));
        }

        [TestMethod]
        public async Task UpdateAuthRole_WrongId_ShouldReturnBadRequest()
        {
            // Arrange
            var roles = await GetRolesAsync();
            var adminRole = roles.Single(r => r.Name == "Admin");
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var putRole = new RolePutDto(adminRole.Id, adminRole.Name);
            var putContent = JsonConvert.SerializeObject(putRole);
            var bodyContent = new StringContent(putContent, Encoding.UTF8, "application/json");
            // Act
            var putResponse = await client.PutAsync($"{BaseUrl}/put/1234", bodyContent);
            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, putResponse.StatusCode);
            var responseContent = await putResponse.Content.ReadAsStringAsync();
            Assert.IsTrue(responseContent.Contains("ObjectId is different from id"));
        }

        [TestMethod]
        public async Task UpdateAuthRole_ExistingRoleName_ShouldReturnBadRequest()
        {
            // Arrange
            var roles = await GetRolesAsync();
            var adminRole = roles.Single(r => r.Name == "Admin");
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var putRole = new RolePutDto(adminRole.Id, "User");
            var putContent = JsonConvert.SerializeObject(putRole);
            var bodyContent = new StringContent(putContent, Encoding.UTF8, "application/json");
            // Act
            var putResponse = await client.PutAsync($"{BaseUrl}/put/{putRole.Id}", bodyContent);
            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, putResponse.StatusCode);
            var responseContent = await putResponse.Content.ReadAsStringAsync();
            Assert.IsTrue(responseContent.Contains("Role name 'User' is already taken"));
        }

        [TestMethod]
        public async Task UpdateAuthRole_NewCorrectRoleName_ShouldReturnCorrectRole()
        {
            // Arrange
            var roles = await GetRolesAsync();
            var adminRole = roles.Single(r => r.Name == "Admin");
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var putRole = new RolePutDto(adminRole.Id, "Administrator");
            var putContent = JsonConvert.SerializeObject(putRole);
            var bodyContent = new StringContent(putContent, Encoding.UTF8, "application/json");
            // Act
            var putResponse = await client.PutAsync($"{BaseUrl}/put/{putRole.Id}", bodyContent);
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);
            var responseContent = await putResponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RoleGetDto>(responseContent);
            Assert.IsNotNull(result);   
            Assert.AreEqual("Administrator", result.Name);
        }


        [TestMethod]
        public async Task DeleteAuthRole_CorrectId_ShouldDeleteRole()
        {
            // Arrange
            var roles = await GetRolesAsync();
            var userRole = roles.Single(r => r.Name == "User");
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var deleteResponse = await client.DeleteAsync($"{BaseUrl}/delete/{userRole.Id}");
            Assert.AreEqual(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        [TestMethod]
        public async Task DeleteRole_IncorrectId_ShouldReturnNotFound()
        {
            var client = await ApiTestHelpers.GetAuthorizedClientAsync();
            var deleteResponse = await client.DeleteAsync($"{BaseUrl}/99");
            Assert.AreEqual(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        }

        [TestMethod]
        public async Task DeleteRole_Unauthorized_ShouldReturnUnauthorized()
        {
            var roles = await GetRolesAsync();
            var userRole = roles.Single(r => r.Name == "User");
            var client = ApiTestHelpers.GetClient();
            var deleteResponse = await client.DeleteAsync($"{BaseUrl}/delete/{userRole.Id}");
            Assert.AreEqual(HttpStatusCode.Unauthorized, deleteResponse.StatusCode);
        }

    }
}
