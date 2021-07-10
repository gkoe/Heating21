using System.Linq;
using System.Threading.Tasks;

using Common.Contracts.Persistence;
using Common.Persistence.Entities;

using Microsoft.EntityFrameworkCore;

namespace Common.Persistence.Repositories
{
    public class SessionRepository : GenericRepository<Session>, ISessionRepository
    {
        private readonly CommonApplicationDbContext _dbContext;

        public SessionRepository(CommonApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Session> GetLastByUserAsync(string userId)
        {
            return await _dbContext.Sessions
                    .Where(s => s.ApplicationUserId == userId)
                    .OrderByDescending(s => s.Login)
                    .FirstOrDefaultAsync();

        }

    }
}
