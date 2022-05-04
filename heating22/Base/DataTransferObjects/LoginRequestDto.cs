
using Base.Validations;

using System.ComponentModel.DataAnnotations;

namespace Base.DataTransferObjects
{
    public class LoginRequestDto
    {
        public LoginRequestDto(string email, string password)
        {
            Email = email;
            Password = password;
        }
        public LoginRequestDto()
        {

        }

        [Required(ErrorMessage = "UserName is required")]
        [RegularExpression("^[a-zA-Z0-9_.-]+@[a-zA-Z0-9-]+.[a-zA-Z0-9-.]+$", ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [CheckPasswordRules]
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}
