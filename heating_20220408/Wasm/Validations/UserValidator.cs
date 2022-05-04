using Base.DataTransferObjects;

using FluentValidation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wasm.Validations
{
    public class UserValidator : AbstractValidator<UserDetailsDto>
    {
        public UserValidator()
        {
            RuleFor(u => u.Email)
                .NotNull()
                .EmailAddress();
            RuleFor(u => u.PhoneNumber)
                .Matches(@"^\+?([0-9\s]){6,18}$")
                .WithMessage("Illegales Format für Telefonnummer");
            RuleFor(u => u.Name)
                .NotNull()
                .Length(2, 10);
            RuleFor(u => u.NewPassword)
                .Cascade(CascadeMode.Stop)
                .MinimumLength(8)
                .Must(IsValidPassword)
                .WithMessage("muss Kleinbuchstaben, Großbuchstaben, Ziffern und Sonderzeichen enthalten");
            RuleFor(u => u).Must(IsModelValid).WithMessage("Password und Confirmpassword müssen übereinstimmen");

        }

        private static bool IsValidPassword(string password)
        {
            if (password == null)
            {
                return true;
            }
            if (!password.Any(ch => char.IsLetter(ch) && char.ToUpper(ch) != ch))  // kein Kleinbuchstabe
            {
                return false;
            }
            if (!password.Any(ch => char.IsLetter(ch) && char.ToLower(ch) != ch))  // kein Großbuchstabe
            {
                return false;
            }
            if (!password.Any(ch => char.IsDigit(ch)))  // keine Ziffer
            {
                return false;
            }
            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))  // kein sonstiges Zeichen
            {
                return false;
            }
            return true;
        }

        protected static bool IsModelValid(UserDetailsDto user)
        {
            if (string.IsNullOrEmpty(user.NewPassword))
            {
                return true;
            }
            return user.NewPassword == user.ConfirmPassword;
        }

    }
}
