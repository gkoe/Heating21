using System.Threading.Tasks;

using Base.DataTransferObjects;
using Base.Entities;

namespace Base.Contracts.Persistence
{
    public interface IApplicationUserRepository
    {
        Task<ApplicationUser> GetByUserIdAsync(string applicationUserId);
        Task<ApplicationUser> FindByEmailAsync(string mail);
        Task<ApplicationUser[]> GetByUserIdAsync();
        Task<UserDetailsDto[]> GetWithRolesAndLastLogin();
        Task<UserGetDto> GetUserDto(string userId);
    }
}
