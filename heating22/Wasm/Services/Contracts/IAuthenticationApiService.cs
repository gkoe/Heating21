using Base.DataTransferObjects;

using System.Threading.Tasks;

namespace Wasm.Services.Contracts
{
    public interface IAuthenticationApiService
    {
        Task<ApiResponseDto<UserGetDto>> RegisterUser(RegisterRequestDto registerRequestDto);

        Task<ApiResponseDto<LoginResponseDto>> Login(LoginRequestDto userFromAuthentication);

        Task<ApiResponseDto> Logout();
        //Task<ApplicationUser[]> GetApplicationUsersAsync();

        Task<ApiResponseDto<UserDetailsDto[]>> GetUsersWithRolesAsync();

        Task<ApiResponseDto<UserGetDto>> UpsertUserAsync(UserPutDto user);
        Task<ApiResponseDto<UserGetDto>> UpdateUserAsync(UserPutDto user);

        Task<ApiResponseDto> DeleteUserAsync(string userId);
        Task<UserGetDto> GetUserAsync(string userId);
    }
}
