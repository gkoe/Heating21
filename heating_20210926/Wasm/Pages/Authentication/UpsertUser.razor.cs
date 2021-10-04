using Blazored.FluentValidation;
using Blazored.LocalStorage;

using Common.DataTransferObjects;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using Radzen;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Wasm.Services;
using Wasm.Services.Contracts;
using Wasm.Validations;
using Wasm.Helper;
using Common.Helper;

namespace Wasm.Pages.Authentication
{
    public partial class UpsertUser
    {
        //[Parameter]
        public UserDetailsDto FormUser { get; set; }

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
        public static String[] Roles => new string[] { "Admin", "Retailer", "Tipper" };
        public string ModelError { get; set; } = "";
        public bool HasChanges { get; set; }
        public bool HasErrors { get; set; }

        public string IsSaveButtonDisabled => (!HasChanges || HasErrors) ? "disabled" : null;
        public string IsCancelButtonDisabled => (!HasChanges) ? "disabled" : null;

        protected override async Task OnInitializedAsync()
        {
            //await Task.Delay(2000);
            base.OnInitialized();
            FormUser = await LocalStorage.GetItemAsync<UserDetailsDto>(nameof(UserDetailsDto));
            MiniMapper.CopyProperties(OriginalUser, FormUser);
            EditContext = new EditContext(FormUser);
            EditContext.OnFieldChanged += EditContext_OnFieldChanged;
        }

        private void EditContext_OnFieldChanged(object sender, FieldChangedEventArgs e)
        {
            HasChanges = MiniMapper.AnyPropertyValuesDifferent(OriginalUser, FormUser);
            StateHasChanged();

            UserValidator userValidator = new ();
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
                NotificationService.ShowNotification(NotificationSeverity.Error, "Save Userdata failed", 
                            result.Errors.ToArray());
            }
        }

        void Cancel()
        {
            NavigationManager.NavigateTo("/users", false);
        }

    }
}
