using Base.Helper;

using Blazored.LocalStorage;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

using Wasm.Services;

namespace Wasm.Shared
{
    public partial class NavMenu
    {
        public string UsersText { get; set; }

        [Inject]
        public ILocalStorageService LocalStorage { get; set; }
        [Inject]
        public AuthenticationStateProvider AuthStateProvider { get; set; }

        public bool IsAdminSignedIn { get; set; } = false;

        protected async override Task OnInitializedAsync()
        {
            Console.WriteLine("NavMenu OnInitAsync()");
            //await Task.Delay(2000);
            var token = await LocalStorage.GetItemAsync<string>(MagicStrings.Local_Token);
            if (token != null)
            {
                var handler = new JwtSecurityTokenHandler();
                var decodedToken = handler.ReadJwtToken(token);
                if (decodedToken.ValidTo < DateTime.UtcNow)
                {
                    (AuthStateProvider as MyAuthenticationStateProvider).NotifyUserLogout();
                    Console.WriteLine($"******* NavMenu, User logged out with AuthStateProvider");
                }
                else
                {
                    var adminRoleClaim = decodedToken.Claims.Where(c => c.Value == MagicStrings.Role_Admin && c.Type.Contains("role")).FirstOrDefault();
                    IsAdminSignedIn = adminRoleClaim != null;
                }

                //if(decodedToken.ValidTo < DateTime.Now) // Token aus localstorage löschen (UTC beachten)
            }
            await base.OnInitializedAsync();
        }

        //private bool IsSignedInAsAdmin()
        //{
        //    //await Task.Delay(5000);
        //    var token = LocalStorage.GetItemAsync<string>(MagicStrings.Local_Token).Result;
        //    var handler = new JwtSecurityTokenHandler();
        //    var decodedToken = handler.ReadJwtToken(token);
        //    var adminRoleClaim = decodedToken.Claims.Where(c => c.Value == MagicStrings.Role_Admin && c.Type.Contains("role")).FirstOrDefault();
        //    return adminRoleClaim != null;
        //}


        //private async Task OnReadyStateChange(EventArgs e)
        //{
        //    var token = await LocalStorage.GetItemAsync<string>(MagicStrings.Local_Token);
        //    var handler = new JwtSecurityTokenHandler();
        //    var decodedToken = handler.ReadJwtToken(token);
        //    var adminRoleClaim = decodedToken.Claims.Where(c => c.Value == MagicStrings.Role_Admin && c.Type.Contains("role")).FirstOrDefault();
        //}

    }
}
