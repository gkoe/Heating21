namespace Base.DataTransferObjects
{
    public class LoginResponseDto
    {
        public LoginResponseDto(string authToken, string email)
        {
            AuthToken = authToken;
            Email = email;
        }

        public string AuthToken { get; set; }
        public string Email { get; set; }
    }
}
