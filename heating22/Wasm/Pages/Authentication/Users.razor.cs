using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using Wasm.Services.Contracts;
using Radzen;
using Radzen.Blazor;
using System.Linq;
using System.Collections.Generic;
using Microsoft.JSInterop;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Wasm.Services;
using Base.DataTransferObjects;
using Base.Helper;

namespace Wasm.Pages.Authentication
{
    public partial class Users
    {
        [Inject]
        public IAuthenticationApiService AuthenticationApiService { get; set; }
        //[Inject]
        //public IAuthorizationService AuthorizationService { get; set; }

        public AuthenticationStateProvider AuthStateProvider { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public ILocalStorageService LocalStorage { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public UtilityServices UtilityServices { get; set; }


        public RadzenGrid<UserDetailsDto> UsersGrid { get; set; }

        public int Count => ApplicationUsers.Count;
        List<UserDetailsDto> ApplicationUsers { get; set; } = new List<UserDetailsDto>();
        public static String[] Roles => new string[] { MagicStrings.Role_Admin, MagicStrings.Role_User, MagicStrings.Role_Guest };
        private string DeleteUserId { get; set; } = null;
        private bool IsProcessing { get; set; } = false;

        public UserDetailsDto FormUser { get; set; } = new UserDetailsDto();

        public bool HasChanges { get; set; }

        public bool IsInAdd { get; set; }

        public object SelectedUser { get; set; }

        protected async override Task OnInitializedAsync()
        {
            //await Task.Delay(2000);
            await base.OnInitializedAsync();
            ApplicationUsers = new List<UserDetailsDto>();
            var apiResult = await AuthenticationApiService.GetUsersWithRolesAsync();
            if (!apiResult.IsSuccessful)
            {
                Console.WriteLine($"LoadUsers, error from api: {apiResult.ErrorsText}");
                NavigationManager.NavigateTo("/login");
            }
            ApplicationUsers = apiResult.Result.ToList();
        }

        async Task EditUser(UserDetailsDto user)
        {
            await LocalStorage.SetItemAsync(nameof(UserDetailsDto), user);
            NavigationManager.NavigateTo("/upsert", true);
        }

        async Task AddUser()
        {
            var user = new UserDetailsDto();
            await LocalStorage.SetItemAsync(nameof(UserDetailsDto), user);
            NavigationManager.NavigateTo("/upsert", true);
        }

        async Task DeleteConfirmation(UserDetailsDto user)
        {
            bool ok = (await DialogService.Confirm("Are you sure to delete user?", "Delete User", new ConfirmOptions() { OkButtonText = "Yes", CancelButtonText = "No" })).GetValueOrDefault();
            if (ok)
            {
                var result = await AuthenticationApiService.DeleteUserAsync(user.Id);
                if (result.IsSuccessful)
                {
                    ApplicationUsers.Remove(user);
                    await UsersGrid.Reload();
                }
                else
                {
                    UtilityServices.ShowNotification(NotificationSeverity.Error, "Delete User failed", result.Errors.ToArray());
                    NavigationManager.NavigateTo("/users", true);
                }

            }
        }

    }
}
