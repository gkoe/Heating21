using System.Threading.Tasks;

using Common.Persistence.Entities;

namespace Common.Contracts.Persistence
{
    public interface ISessionRepository : IGenericRepository<Session>
    {
        Task<Session> GetLastByUserAsync(string userId);
    }
}
