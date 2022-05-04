
using Base.Validations;

using System;
using System.ComponentModel.DataAnnotations;

namespace Base.DataTransferObjects
{
    public class UserPutDto
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }
        
        [Required, EmailAddress]
        public string Email { get; set; }

        public string RoleName { get; set; }
        public string PhoneNumber { get; set; } = String.Empty;

        [CheckPasswordRules]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; } = string.Empty;

        [CheckPasswordRules]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"User: {Name}, {Email}";
        }
    }
}
