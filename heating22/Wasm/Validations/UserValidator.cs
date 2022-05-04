
using Base.DataTransferObjects;
using Base.Validations;

using FluentValidation;
using FluentValidation.Validators;

using System.Linq;

using Wasm.DataTransferObjects;

namespace Wasm.Validations
{
    public class UserValidator : AbstractValidator<WasmUserPutDto>
    {
        public static string PasswordErrorMessage { get; private set; } = string.Empty;

        public UserValidator()
        {
            RuleFor(u => u.Email)
                .NotNull()
                .EmailAddress()
                .WithMessage("Mailadresse ungültig");
            RuleFor(u => u.PhoneNumber)
                .Matches(@"^\+?([0-9\s]){6,18}$")
                .WithMessage("Illegales Format für Telefonnummer");
            RuleFor(u => u.Name)
                .NotNull()
                .Length(2, 10)
                .WithMessage("Länge muss zwischen 2 und 10 Zeichen sein");
            RuleFor(u => u.OldPassword)
                .Cascade(CascadeMode.Stop)
                .Must(IsValidPassword)
                .WithMessage(pw => string.Format("{0}", PasswordErrorMessage));
            RuleFor(u => u.NewPassword)
                .Cascade(CascadeMode.Stop)
                .Must(IsValidPassword)
                .WithMessage(pw => string.Format("{0}", PasswordErrorMessage));
            //.WithMessage("Mindestlänge 8 Zeichen, muss Kleinbuchstaben, Großbuchstaben, Ziffern und Sonderzeichen enthalten");
            RuleFor(u => u).Must(IsModelValid)
                .WithMessage("Password und Confirmpassword müssen übereinstimmen");

        }

        private static bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return true;
            }
            // Verwendung der Validation des Passworts im Model
            var errorMessage = CheckPasswordRules.GetErrorMessageForPasswordRules(password);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                PasswordErrorMessage = errorMessage;
                return false;
            }
            return true;
        }

        protected static bool IsModelValid(WasmUserPutDto user)
        {
            if (string.IsNullOrEmpty(user.NewPassword) || (string.IsNullOrEmpty(user.ConfirmPassword)))
            {
                return true;
            }
            return user.NewPassword == user.ConfirmPassword;
        }


        //public static IRuleBuilderOptionsConditions<T, string> PasswordValitatorRule<T, TElement>(this IRuleBuilder<T, string> ruleBuilder)
        //{

        //    return ruleBuilder.Custom((password, context) => {
        //        if (!string.IsNullOrEmpty(password))
        //        {
        //            var errorMessage = CheckPasswordRules.GetErrorMessageForPasswordRules(password);
        //            if (!string.IsNullOrEmpty(errorMessage))
        //            {
        //                context.AddFailure(errorMessage);
        //            }
        //        }
        //    });
        //}

    }



    //public class PasswordValidator : AbstractValidator<string>
    //{
    //    public PasswordValidator()
    //    {
    //        RuleFor(pw => pw).Custom((password, context) => {
    //            if (!string.IsNullOrEmpty(password))
    //            {
    //                var errorMessage = CheckPasswordRules.GetErrorMessageForPasswordRules(password);
    //                if (!string.IsNullOrEmpty(errorMessage))
    //                {
    //                    context.AddFailure(errorMessage);
    //                }
    //            }
    //        });
    //    }
    //}

}
