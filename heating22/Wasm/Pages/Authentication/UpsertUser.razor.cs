using Blazored.LocalStorage;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using Radzen;

using System;
using System.Threading.Tasks;
using Wasm.Services.Contracts;
using Wasm.Validations;
using Base.DataTransferObjects;
using Base.Helper;
using Wasm.DataTransferObjects;

namespace Wasm.Pages.Authentication
{
    public partial class UpsertUser
    {
        //[Parameter]
        public WasmUserPutDto FormUser { get; set; }

        [Inject]
        public IAuthenticationApiService AuthenticationService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public ILocalStorageService LocalStorage { get; set; }
        //[Inject]
        //public UtilityServices UtilityServices { get; set; }
        [Inject]
        public NotificationService NotificationService { get; set; }


        public UserDetailsDto OriginalUser { get; set; } = new UserDetailsDto();
        public EditContext EditContext { get; set; }
        public static String[] Roles => new string[] { MagicStrings.Role_Admin, MagicStrings.Role_User, MagicStrings.Role_Guest };
        public string ModelError { get; set; } = "";
        public bool HasChanges { get; set; }
        public bool HasErrors { get; set; }

        //public string IsSaveButtonDisabled => (!HasChanges || HasErrors) ? "disabled" : null;
        public string IsSaveButtonDisabled => (!HasChanges ) ? "disabled" : null;
        public string IsCancelButtonDisabled => (!HasChanges) ? "disabled" : null;

        protected override async Task OnInitializedAsync()
        {
            //await Task.Delay(2000);
            base.OnInitialized();
            var userDetail = await LocalStorage.GetItemAsync<UserDetailsDto>(nameof(UserDetailsDto));
            FormUser = new WasmUserPutDto
            {
                Id = userDetail.Id,
                Name = userDetail.Name,
                Email = userDetail.Email,
                PhoneNumber = userDetail.PhoneNumber,
                RoleName = userDetail.RoleName,
            };
            MiniMapper.CopyProperties(OriginalUser, FormUser);
            EditContext = new EditContext(FormUser);
            EditContext.OnFieldChanged += EditContext_OnFieldChanged;
        }

        private void EditContext_OnFieldChanged(object sender, FieldChangedEventArgs e)
        {
            HasChanges = MiniMapper.AnyPropertyValuesDifferent(OriginalUser, FormUser);
            StateHasChanged();

            UserValidator userValidator = new();
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
            //! wenn email geändert wurde ==> per api auf unique überprüfen, derzeit nach save
        }

        async Task Save()
        {
            var result = await AuthenticationService.UpsertUserAsync(FormUser);
            if (result.IsSuccessful)
            {
                NavigationManager.NavigateTo("/users", false);
            }
            else
            {
                HasErrors = true;
                ModelError = $"{result.ErrorsText}";

                //NotificationService.ShowNotification(NotificationSeverity.Error, "Save Userdata failed", 
                            //result.Errors.ToArray());
            }
        }

        void Cancel()
        {
            NavigationManager.NavigateTo("/users", false);
        }

    }
}
