using System.Threading.Tasks;

using Common.DataTransferObjects;
using Common.Persistence.Entities;

namespace Common.Contracts.Persistence
{
    public interface IApplicationUserRepository
    {
        Task<ApplicationUser> GetByUserIdAsync(string applicationUserId);
        Task<ApplicationUser> FindByEmailAsync(string mail);
        Task<ApplicationUser[]> GetByUserIdAsync();
        Task<UserDetailsDto[]> GetWithRolesAndLastLogin();
        Task<UserDto> GetUserDto(string userId);
    }
}
