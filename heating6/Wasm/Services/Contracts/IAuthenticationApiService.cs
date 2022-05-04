using System.Threading.Tasks;
using Base.DataTransferObjects;

namespace Wasm.Services.Contracts
{
    public interface IAuthenticationApiService
    {
        Task<ApiResponseDto> RegisterUser(RegisterRequestDto userForRegisteration);

        Task<LoginResponseDto> Login(LoginRequestDto userFromAuthentication);

        Task<ApiResponseDto> Logout();
        //Task<ApplicationUser[]> GetApplicationUsersAsync();

        Task<UserDetailsDto[]> GetUsersWithRolesAsync();

        Task<ApiResponseDto> UpsertUserAsync(UserDetailsDto user);
        Task<ApiResponseDto> EditUserAsync(UserDetailsDto user);

        Task<ApiResponseDto> DeleteUserAsync(string userId);
        Task<UserDto> GetUserAsync(string userId);
    }
}
