using System;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Heating.Core.Contracts
{
    public interface IUnitOfWork : IDisposable
    {
        DbContext DbContext { get; }

        ISensorRepository Sensors { get; }
        IMeasurementRepository Measurements { get; }
        Task<int> SaveChangesAsync();
        Task DeleteDatabaseAsync();
        Task MigrateDatabaseAsync();
        bool ContextHasChanges();
        void Reload(IEntityObject entity);
        void ClearChangeTracker();
    }
}
