using Blazored.LocalStorage;

using Common.Helper;

using Microsoft.AspNetCore.Components.Authorization;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Wasm.Services
{
    public class MyAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public MyAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            Console.WriteLine($"**** MyAuthenticationStateProvider, Constructor");
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>(MagicStrings.Local_Token);
            if (token == null)
            {
                Console.WriteLine($"**** GetAuthState: no token");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            var exp = JwtParser.ParseExpirationTimeFromJwt(token);
            if (exp < DateTime.Now)
            {
                Console.WriteLine($"**** GetAuthState: token expired  ==> user logged out");
                await _localStorage.RemoveItemAsync(MagicStrings.Local_Token);
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token),
                    "jwtAuthType"));
            Console.WriteLine($"**** GetAuthState: authenticated: {principal.Identity.IsAuthenticated}");
            return new AuthenticationState(principal);
        }

        public void NotifyUserLoggedIn(string token)
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token),
                        "jwtAuthType"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            Console.WriteLine($"**** NotifyUserLoggedIn: authenticated: {authenticatedUser.Identity.Name}");
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            Console.WriteLine($"**** NotifyUserLogout");
            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            NotifyAuthenticationStateChanged(authState);
        }
    }
}
