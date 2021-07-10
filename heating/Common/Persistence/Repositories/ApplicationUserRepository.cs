using System.Linq;
using System.Threading.Tasks;

using Common.Contracts.Persistence;
using Common.DataTransferObjects;
using Common.Persistence.Entities;

using Microsoft.EntityFrameworkCore;

namespace Common.Persistence.Repositories
{
    public class ApplicationUserRepository : IApplicationUserRepository
    {
        private readonly CommonApplicationDbContext _dbContext;

        public ApplicationUserRepository(CommonApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApplicationUser> FindByEmailAsync(string mail)
        {
            return await _dbContext.ApplicationUsers
                .FirstOrDefaultAsync(u => u.Email == mail);
        }

        public async Task<ApplicationUser> GetByUserIdAsync(string applicationUserId)
        {
            return await _dbContext.ApplicationUsers
                            .FirstOrDefaultAsync(u => u.Id == applicationUserId);
        }

        public async Task<ApplicationUser[]> GetByUserIdAsync()
        {
            return await _dbContext.ApplicationUsers
                .OrderBy(u => u.Name)
                .ToArrayAsync();
        }

        public async Task<UserDetailsDto[]> GetWithRolesAndLastLogin()
        {
            var users = await _dbContext
                .ApplicationUsers
                .OrderBy(u => u.UserName)
                .ToArrayAsync();
            var roles = await _dbContext.Roles.ToArrayAsync();

            var allUserRoles = await _dbContext.UserRoles.ToArrayAsync();
            var userRoles = allUserRoles
                            .Select(ur => new
                            {
                                ur.UserId,
                                RoleName = roles.First(r => r.Id == ur.RoleId).Name
                            })
                            .ToArray();
            var sessions = await _dbContext
                .ApplicationUsers
                .Select(u => u.Sessions
                        .OrderByDescending(s => s.Login)
                        .FirstOrDefault())
                .Where(s => s != null)
                .ToListAsync();
            var result = users.Select(u => new UserDetailsDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = userRoles.FirstOrDefault(ur => ur.UserId == u.Id)?.RoleName,
                LastLogin = sessions.FirstOrDefault(s => s.ApplicationUserId == u.Id)?.Login
            })
            .ToArray();
            return result;

        }

        public async Task<UserDto> GetUserDto(string userId)
        {
            var user = await _dbContext.ApplicationUsers
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();

            UserDto userDto = new UserDto
            {
                Name = user.Name,
                Email = user.Email,
                Id = userId
            };
            return userDto;

        }

        //private string GetRolesForUser(string userId, IdentityRole[] roles, IdentityUserRole<string>[] userRoles)
        //{
        //    var r = userRoles.Where(ur => ur.UserId == userId)
        //        .Select(x => roles.Single(r => r.Id == x.RoleId))
        //        .Select(x => x.Name)
        //        .ToArray();
        //    return string.Join(',', r);
        //}
    }
}
