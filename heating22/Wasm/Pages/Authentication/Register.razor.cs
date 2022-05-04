using Base.DataTransferObjects;

using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wasm.Services.Contracts;

namespace Wasm.Pages.Authentication
{
    public partial class Register
    {
        private RegisterRequestDto UserForRegistration { get; set; } = new RegisterRequestDto();
        public bool IsProcessing { get; set; } = false;
        public bool ShowRegistrationErrors { get; set; }
        public IEnumerable<string> Errors { get; set; }

        [Inject]
        public IAuthenticationApiService AuthenticationService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private async Task RegisterUser()
        {
            ShowRegistrationErrors = false;
            IsProcessing = true;
            var result = await AuthenticationService.RegisterUser(UserForRegistration);
            if (result.IsSuccessful)
            {
                IsProcessing = false;
                NavigationManager.NavigateTo("/login");
            }
            else
            {
                IsProcessing = false;
                Errors = result.Errors;
                ShowRegistrationErrors = true;
            }
        }
    }
}
