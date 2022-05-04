using Base.Helper;

using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Wasm.Services
{
    public class MyAuthenticationStateProvider : AuthenticationStateProvider
    {
        private  HttpClient HttpClient { get; }
        private ILocalStorageService LocalStorage { get; }

        public MyAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
        {
            HttpClient = httpClient;
            LocalStorage = localStorage;
            Console.WriteLine($"**** MyAuthenticationStateProvider, Constructor");
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await LocalStorage.GetItemAsync<string>(MagicStrings.Local_Token);
            if (token == null)
            {
                Console.WriteLine($"**** GetAuthState: no token");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            var exp = JwtParser.ParseExpirationTimeFromJwt(token);
            if (exp < DateTime.Now)
            {
                Console.WriteLine($"**** GetAuthState: token expired  ==> user logged out");
                await LocalStorage.RemoveItemAsync(MagicStrings.Local_Token);
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
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

        public static bool IsAdmin(AuthenticationState authState)
        {
            var claims = authState.User.Claims.ToArray();
            foreach (var claim in claims)
            {
                if (claim.Type.Contains("claims/role")  && claim.Value.ToUpper() == "ADMIN")
                {
                    return true;
                }
            }
            return false;
        }

        public void NotifyUserLogout()
        {
            Console.WriteLine($"**** NotifyUserLogout");
            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            NotifyAuthenticationStateChanged(authState);
        }
    }
}
