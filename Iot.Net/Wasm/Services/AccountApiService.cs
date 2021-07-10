using Blazored.LocalStorage;

using Common.DataTransferObjects;
using Common.Helper;

using Microsoft.AspNetCore.Components.Authorization;

using Newtonsoft.Json;

using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Wasm.Services.Contracts;

namespace Wasm.Services
{
    public class AccountApiService : IAuthenticationApiService
    {
        private readonly HttpClient _client;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AccountApiService(HttpClient client, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
        {
            _client = client;
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
        }

        public async Task<UserDto> GetUserAsync(string userId)
        {
            var response = await _client.GetAsync($"api/account/getbyid/{userId}");
            var contentTemp = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<UserDto>(contentTemp);
            return result;
        }

        public async Task<UserDetailsDto[]> GetUsersWithRolesAsync()
        {
            var response = await _client.GetAsync("api/account/get");
            var contentTemp = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<UserDetailsDto[]>(contentTemp);
            return result;
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto userFromAuthentication)
        {
            var content = JsonConvert.SerializeObject(userFromAuthentication);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("api/account/login", bodyContent);
            var contentTemp = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<LoginResponseDto>(contentTemp);

            if (response.IsSuccessStatusCode)
            {
                await _localStorage.SetItemAsync(MagicStrings.Local_Token, result.Token);
                //await _localStorage.SetItemAsync(MagicStrings.Local_UserDetails, result.User);
                ((MyAuthenticationStateProvider)_authStateProvider).NotifyUserLoggedIn(result.Token);
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Token);
                return new LoginResponseDto { IsSuccessful = true };
            }
            else
            {
                return result;
            }
        }

        public async Task<ApiResponseDto> Logout()
        {
            //UserDto userDto = await _localStorage.GetItemAsync<UserDto>(MagicStrings.Local_UserDetails);
            var token = await _localStorage.GetItemAsync<string>(MagicStrings.Local_Token);
            if (token == null)
            {
                return new ApiResponseDto { IsSuccessful = false, Errors = new string[] { "logout failed, no token" } };
            }
            string id = JwtParser.ParseIdFromJwt(token);
            if (id == null)
            {
                return new ApiResponseDto { IsSuccessful = false, Errors = new string[] { "logout failed, no id in token" } };
            }
            var response = await _client.GetAsync($"api/account/logout/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponseDto { IsSuccessful = false, Errors = new string[] { response.StatusCode.ToString() } };
            }
            await _localStorage.RemoveItemAsync(MagicStrings.Local_Token);
            //await _localStorage.RemoveItemAsync(MagicStrings.Local_UserDetails);
            _client.DefaultRequestHeaders.Authorization = null;
            ((MyAuthenticationStateProvider)_authStateProvider).NotifyUserLogout();
            return new ApiResponseDto { IsSuccessful = true };
        }

        public async Task<ApiResponseDto> RegisterUser(RegisterRequestDto userForRegisteration)
        {
            var content = JsonConvert.SerializeObject(userForRegisteration);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("api/account/register", bodyContent);
            var contentTemp = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ApiResponseDto>(contentTemp);

            if (response.IsSuccessStatusCode)
            {

                return new ApiResponseDto { IsSuccessful = true };
            }
            else
            {
                return result;
            }
        }

        public async Task<ApiResponseDto> EditUserAsync(UserDetailsDto user)
        {
            var content = JsonConvert.SerializeObject(user);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("api/account/edituser", bodyContent);

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponseDto { IsSuccessful = true };
            }
            else
            {
                var contentTemp = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponseDto>(contentTemp);
                var errors = result?.Errors.Select(e => e).ToArray();
                return new ApiResponseDto { Errors = errors, IsSuccessful = false };
            }
        }

        public async Task<ApiResponseDto> UpsertUserAsync(UserDetailsDto user)
        {
            var content = JsonConvert.SerializeObject(user);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("api/account/upsertbyadmin", bodyContent);

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponseDto { IsSuccessful = true };
            }
            else
            {
                var contentTemp = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponseDto>(contentTemp);
                var errors = result?.Errors.Select(e => e).ToArray();
                return new ApiResponseDto { Errors = errors, IsSuccessful = false };
            }
        }

        public async Task<ApiResponseDto> DeleteUserAsync(string userId)
        {
            var content = JsonConvert.SerializeObject(userId);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("api/account/delete", bodyContent);
            var contentTemp = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ApiResponseDto>(contentTemp);

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponseDto { IsSuccessful = true };
            }
            else
            {
                var errors = new string[] { result?.ToString() };
                return new ApiResponseDto { Errors = errors, IsSuccessful = false };
            }
        }
    }

}
