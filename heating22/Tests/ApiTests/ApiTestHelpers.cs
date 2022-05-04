using Api;

using Base.DataTransferObjects;

using Core.Contracts;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using Persistence;

using Serilog;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Tests.ApiTests
{
    public static class ApiTestHelpers
    {
        const string UsersBaseUrl = "/api/users";

        public static readonly LoginRequestDto admin = new ("admin@htl.at", "Admin123*");
        public static readonly LoginRequestDto user = new ("user@htl.at", "User123*");

        public static HttpClient GetClient()
        {
            var webBuilder = new WebHostBuilder();
            (webBuilder as IHostBuilder).UseSerilog();
            TestServer server = new(webBuilder.UseStartup<Startup>());
            HttpClient client = server.CreateClient();
            return client;
        }

        /// <summary>
        /// Get user by email, or empty user if user with email doesn't exist.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static async Task<UserGetDto> GetUserByEmail(string email)
        {
            var adminClient = await GetAuthorizedClientAsync();
            var response = await adminClient.GetAsync($"{UsersBaseUrl}/getbyemail/{email}");

            var responseContent = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<UserGetDto>(responseContent);

            return user ?? new UserGetDto("", "", "", "", ""); // If null, return empty user
        }

        public static async Task<HttpClient> GetAuthorizedClientAsync(LoginRequestDto? credentials = null)
        {
            var client = GetClient();
            var apiLogin = $"{UsersBaseUrl}/login";

            credentials ??= admin;
            var json = JsonConvert.SerializeObject(credentials);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await client.PostAsync(apiLogin, content);
            //if (resp.StatusCode != HttpStatusCode.OK)
            //{
            //    return null;
            //}
            Assert.AreEqual(resp.StatusCode, HttpStatusCode.OK);
            var respContent = await resp.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<LoginResponseDto>(respContent);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result?.AuthToken);
            return client;
        }


        public async static Task RecreateDatabaseAsync()
        {
            using ApplicationDbContext dbContext = new();
            using IUnitOfWork uow = new UnitOfWork(dbContext);
            await uow.DeleteDatabaseAsync();
            await uow.CreateDatabaseAsync(); 
        }
    }
}
