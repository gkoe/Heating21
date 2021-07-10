using Blazored.LocalStorage;
using Common.DataTransferObjects;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;

using Wasm.Services;
using Wasm.Services.Contracts;
using Wasm.Validations;
using Radzen;
using Common.Helper;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Wasm.Pages.Authentication
{
    public partial class EditUser
    {
        [Inject]
        public IAuthenticationApiService AuthenticationApiService { get; set; }

        [Inject]
        public IAuthorizationService AuthorizationService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public ILocalStorageService LocalStorage { get; set; }
        [Inject]
        public UtilityServices UtilityServices { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }


        public UserDetailsDto FormUser { get; set; }
        public UserDetailsDto AuthenticatedUser { get; set; } = new UserDetailsDto();
        public EditContext EditContext { get; set; }
        public static String[] Roles => new string[] { "Admin", "Retailer", "Tipper" };

        public string ModelError { get; set; } = "";
        public bool HasChanges { get; set; }
        public bool HasErrors { get; set; }

        public string IsSaveButtonDisabled => (!HasChanges || HasErrors) ? "disabled" : null;
        public string IsCancelButtonDisabled => (!HasChanges) ? "disabled" : null;

        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            await Task.Delay(2000);
            var loggedinUser = (await AuthenticationStateTask).User;
            if (loggedinUser.IsInRole("Admin"))
            {
                Console.WriteLine($"User {loggedinUser.Identity.Name} is in role admin");
            }
            //var user = await LocalStorage.GetItemAsync<UserDto>(MagicStrings.Local_UserDetails);
            var token = await LocalStorage.GetItemAsync<string>(MagicStrings.Local_Token);
            var id = JwtParser.ParseIdFromJwt(token);
            var user = await AuthenticationApiService.GetUserAsync(id);
            AuthenticatedUser = new UserDetailsDto();
            MiniMapper.CopyProperties(AuthenticatedUser, user);
            AuthenticatedUser.Role = JwtParser.ParseRolesFromJwt(token).FirstOrDefault();
            FormUser = new UserDetailsDto();
            MiniMapper.CopyProperties( FormUser, AuthenticatedUser);
            EditContext = new EditContext(FormUser);
            EditContext.OnFieldChanged += EditContext_OnFieldChanged;
        }

        private void EditContext_OnFieldChanged(object sender, FieldChangedEventArgs e)
        {
            HasChanges = MiniMapper.AnyPropertyValuesDifferent(AuthenticatedUser, FormUser);
            StateHasChanged();

            UserValidator userValidator = new UserValidator();
            ValidationResult results = userValidator.Validate(FormUser);
            ModelError = "";
            HasErrors = !results.IsValid;
            if (HasErrors)
            {
                HasErrors = true;
                foreach (var failure in results.Errors)
                {
                    if (string.IsNullOrEmpty(failure.PropertyName))
                    {
                        ModelError += $"{failure.ErrorMessage} \n";
                    }
                }
            }
            //! wenn email geändert wurde ==> per api auf unique überprüfen, derzeit bei Save von API
        }

        async Task Save()
        {
            //! Id == null ==> Post sonst put
            var result = await AuthenticationApiService.EditUserAsync(FormUser);
            if (result.IsSuccessful)
            {
                NavigationManager.NavigateTo("/", false);
            }
            else
            {
                UtilityServices.ShowNotification(NotificationSeverity.Error, "Save Userdata failed", result.Errors.ToArray());
            }
        }
        void Cancel()
        {
            NavigationManager.NavigateTo("/users", false);
        }

    }
}
