namespace Base.DataTransferObjects
{
    public class UserGetDto
    {
        public UserGetDto(string id, string email, string name, string roleName, string phoneNumber)
        {
            Id = id;
            Email = email;
            Name = name;
            RoleName = roleName;
            PhoneNumber = phoneNumber;
        }

        public UserGetDto()
        {

        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string RoleName { get; set; }
    }
}
