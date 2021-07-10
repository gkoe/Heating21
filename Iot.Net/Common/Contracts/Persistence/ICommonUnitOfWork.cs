using System;
using System.Threading.Tasks;

using Common.Contracts.Persistence;

namespace Common.Contracts
{
    public interface ICommonUnitOfWork : IDisposable
    {
        ISessionRepository Sessions { get; }
        IApplicationUserRepository ApplicationUsers { get; }

        Task<int> SaveChangesAsync();
        Task DeleteDatabaseAsync();
        Task MigrateDatabaseAsync();
        Task CreateDatabaseAsync();
    }
}
