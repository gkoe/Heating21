using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

using System.Threading.Tasks;

namespace Wasm.Pages
{
    public partial class Index
    {
        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationState { get; set; }


        protected override async Task OnInitializedAsync()
        {
            // await Task.Delay(5000);
            var authState = await AuthenticationState;

            if (authState?.User?.Identity is null || !authState.User.Identity.IsAuthenticated)
            {
                NavigationManager.NavigateTo($"login?returnUrl=overview", true);
            }
        }
    }
}

