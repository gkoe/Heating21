using Base.DataTransferObjects;

using Microsoft.AspNetCore.Components;

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using Wasm.Services.Contracts;

namespace Wasm.Pages.Authentication
{
    public partial class Login
    {
        private LoginRequestDto UserForAuthentication { get; set; } = new LoginRequestDto("", "");
        public bool IsProcessing { get; set; } = false;
        public bool ShowAuthenticationErrors { get; set; }
        public string Errors { get; set; }
        public string ReturnUrl { get; set; }
        [Inject]
        public IAuthenticationApiService AuthenticationApiService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private async Task LoginUser()
        {
            ShowAuthenticationErrors = false;
            IsProcessing = true;
            var result = await AuthenticationApiService.Login(UserForAuthentication);
            if (result.IsSuccessful)
            {
                IsProcessing = false;

                var absoluteUri = new Uri(NavigationManager.Uri);
                var queryParam = HttpUtility.ParseQueryString(absoluteUri.Query);
                ReturnUrl = queryParam["returnUrl"];
                if (string.IsNullOrEmpty(ReturnUrl))
                {
                    NavigationManager.NavigateTo("/", forceLoad:true);
                }
                else
                {
                    NavigationManager.NavigateTo("/" + ReturnUrl, forceLoad: true);
                }
            }
            else
            {
                IsProcessing = false;
                Errors = string.Join(", ", result.Errors.ToArray());
                ShowAuthenticationErrors = true;
            }
        }
    }
}
