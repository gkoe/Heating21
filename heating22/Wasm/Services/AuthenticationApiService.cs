using Base.DataTransferObjects;
using Base.Helper;

using Blazored.LocalStorage;
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
    public class AuthenticationApiService : IAuthenticationApiService
    {
        const string BaseUrl = "api/users/";

        private HttpClient HttpClient { get; }
        private ILocalStorageService LocalStorage { get; }
        private AuthenticationStateProvider AuthStateProvider { get; }

        public AuthenticationApiService(HttpClient client, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
        {
            HttpClient = client;
            AuthStateProvider = authStateProvider;
            LocalStorage = localStorage;
        }

        public async Task<UserGetDto> GetUserAsync(string userId)
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}getbyid/{userId}");
            var contentTemp = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<UserGetDto>(contentTemp);
            return result;
        }

        /// <summary>
        /// Benutzer per API laden
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponseDto<UserDetailsDto[]>> GetUsersWithRolesAsync()
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}getall");
            if (response.IsSuccessStatusCode)
            {
                var contentTemp = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<UserDetailsDto[]>(contentTemp);
                return new ApiResponseDto<UserDetailsDto[]> { IsSuccessful = true, Result = result };
            }
            else
            {
                return new ApiResponseDto<UserDetailsDto[]>
                {
                    IsSuccessful = false,
                    Errors = new string[] { response.StatusCode.ToString() }
                };
            }

        }

        /// <summary>
        /// User meldet sich per API an
        /// Im Erfolgsfall Token im LocalStorage speichern und AuthorizationHeader
        /// für künftige Requests setzen
        /// </summary>
        /// <param name="loginRequestDto"></param>
        /// <returns></returns>
        public async Task<ApiResponseDto<LoginResponseDto>> Login(LoginRequestDto loginRequestDto)
        {
            var content = JsonConvert.SerializeObject(loginRequestDto);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync($"{BaseUrl}login", bodyContent);

            if (response.IsSuccessStatusCode)
            {
                var contentTemp = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<LoginResponseDto>(contentTemp);
                await LocalStorage.SetItemAsync(MagicStrings.Local_Token, result.AuthToken);
                ((MyAuthenticationStateProvider)AuthStateProvider).NotifyUserLoggedIn(result.AuthToken);
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.AuthToken);
                return new ApiResponseDto<LoginResponseDto> { IsSuccessful = true, Result=result };
            }
            else
            {
                return new ApiResponseDto<LoginResponseDto>
                {
                    IsSuccessful = false,
                    Errors = new string[] { response.StatusCode.ToString() }
                };
            }
        }


        /// <summary>
        /// User per API ausloggen.
        /// Token aus Localstorage löschen
        /// Authorizationheader entfernen
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponseDto> Logout()
        {
            //UserDto userDto = await _localStorage.GetItemAsync<UserDto>(MagicStrings.Local_UserDetails);
            var token = await LocalStorage.GetItemAsync<string>(MagicStrings.Local_Token);
            if (token == null)
            {
                return new ApiResponseDto { IsSuccessful = false, Errors = new string[] { "logout failed, no token" } };
            }
            string id = JwtParser.ParseIdFromJwt(token);
            if (id == null)
            {
                return new ApiResponseDto { IsSuccessful = false, Errors = new string[] { "logout failed, no id in token" } };
            }
            var response = await HttpClient.GetAsync($"{BaseUrl}logout/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponseDto { IsSuccessful = false, Errors = new string[] { response.StatusCode.ToString() } };
            }
            await LocalStorage.RemoveItemAsync(MagicStrings.Local_Token);
            HttpClient.DefaultRequestHeaders.Authorization = null;
            ((MyAuthenticationStateProvider)AuthStateProvider).NotifyUserLogout();
            return new ApiResponseDto { IsSuccessful = true };
        }


        /// <summary>
        /// Neuer Benutzer registriert sich selbst.
        /// Er kann sich natürlich keine Rolle vergeben.
        /// </summary>
        /// <param name="registerRequestDto"></param>
        /// <returns></returns>
        public async Task<ApiResponseDto<UserGetDto>> RegisterUser(RegisterRequestDto registerRequestDto)
        {
            var content = JsonConvert.SerializeObject(registerRequestDto);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync($"{BaseUrl}register", bodyContent);

            if (response.IsSuccessStatusCode)
            {
                var contentTemp = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<UserGetDto>(contentTemp);
                return new ApiResponseDto<UserGetDto> { IsSuccessful = true, Result=result };
            }
            else
            {
                return new ApiResponseDto<UserGetDto>
                {
                    IsSuccessful = false,
                    Errors = new string[] { response.StatusCode.ToString() }
                };
            }
        }

        /// <summary>
        /// User kann seine Daten (Name, Email, ...) selbst ändern
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<ApiResponseDto<UserGetDto>> UpdateUserAsync(UserPutDto user)
        {
            var content = JsonConvert.SerializeObject(user);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await HttpClient.PutAsync($"{BaseUrl}update", bodyContent);
            var contentResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<UserGetDto>(contentResponse);
                return new ApiResponseDto<UserGetDto> { IsSuccessful = true, Result = result };
            }
            else
            {
                return new ApiResponseDto<UserGetDto>
                {
                    IsSuccessful = false,
                    Errors = new string[] { contentResponse }
                };
            }
        }

        /// <summary>
        /// Der Admin darf neue Benutzer inklusive Rolle anlegen und auch bestehende Benutzer editieren
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<ApiResponseDto<UserGetDto>> UpsertUserAsync(UserPutDto user)
        {
            var content = JsonConvert.SerializeObject(user);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await HttpClient.PutAsync($"{BaseUrl}upsertbyadmin", bodyContent);
            var contentResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<UserGetDto>(contentResponse);
                return new ApiResponseDto<UserGetDto> { IsSuccessful = true, Result = result };
            }
            else
            {
                return new ApiResponseDto<UserGetDto>
                {
                    IsSuccessful = false,
                    Errors = new string[] { contentResponse }
                };
            }
        }

        public async Task<ApiResponseDto> DeleteUserAsync(string userId)
        {
            var response = await HttpClient.DeleteAsync($"{BaseUrl}delete/{userId}");
            var contentResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponseDto { IsSuccessful = true };
            }
            else
            {
                return new ApiResponseDto
                {
                    IsSuccessful = false,
                    Errors = new string[] { contentResponse }
                };
            }
        }
    }

}
