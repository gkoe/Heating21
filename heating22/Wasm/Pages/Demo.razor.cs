using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Linq;
using Base.Helper;

namespace Wasm.Pages
{
    public partial class Demo
    {
        [Inject]
        public ILocalStorageService LocalStorage { get; set; }
        [Inject]
        public AuthenticationStateProvider MyAuthStateProvider { get; set; }

        public string Name { get; set; }
        public bool IsAuthenticated { get; set; }
        public string AuthenticationType { get; set; }
        public string ExpirationTime { get; set; }
        public string Role { get; set; }


        protected async override Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var authState = await MyAuthStateProvider.GetAuthenticationStateAsync();
            Name = authState.User.Identity.Name;
            IsAuthenticated = authState.User.Identity.IsAuthenticated;
            AuthenticationType = authState.User.Identity.AuthenticationType;
            var token = await LocalStorage.GetItemAsync<string>(MagicStrings.Local_Token);
            if (token != null)
            {
                ExpirationTime = JwtParser.ParseExpirationTimeFromJwt(token).ToString();
                Role = JwtParser.ParseRolesFromJwt(token).FirstOrDefault();
            }


        }
    }
}
