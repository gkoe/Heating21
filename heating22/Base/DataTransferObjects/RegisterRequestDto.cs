
using Base.Validations;

using System.ComponentModel.DataAnnotations;

namespace Base.DataTransferObjects
{
    public class RegisterRequestDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string PhoneNo { get; set; }

        [CheckPasswordRules]
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirm password is not matched")]
        public string ConfirmPassword { get; set; }
    }
}
