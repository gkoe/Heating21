using System;

namespace Common.DataTransferObjects
{
    public class UserDetailsDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public DateTime? LastLogin { get; set; }

        public override string ToString()
        {
            return $"User: {Name}, {Email}, {Role}";
        }
    }
}
