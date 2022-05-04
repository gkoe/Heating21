using Microsoft.AspNetCore.Components;

using Radzen;

using System.Linq;
using System.Threading.Tasks;

using Wasm.Services;
using Wasm.Services.Contracts;

namespace Wasm.Pages.Authentication
{
    public partial class Logout
    {
        [Inject]
        public IAuthenticationApiService AuthenticationApiService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public UtilityServices UtilityServices { get; set; }


        protected async override Task OnInitializedAsync()
        {
            
            var result = await AuthenticationApiService.Logout();
            if (!result.IsSuccessful)
            {
                UtilityServices.ShowNotification(NotificationSeverity.Error, "Logout failed", result.Errors.ToArray());
            }
            UtilityServices.ShowNotification(NotificationSeverity.Info, "Logout", "User logged out");
            NavigationManager.NavigateTo("/");
        }
    }
}
