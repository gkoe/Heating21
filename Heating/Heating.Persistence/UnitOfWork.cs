using System;
using System.Threading.Tasks;

using Heating.Core.Contracts;

using Microsoft.EntityFrameworkCore;

namespace Heating.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        public DbContext DbContext { get; }
        private bool _disposed;

        /// <summary>
        /// ConnectionString kommt aus den appsettings.json
        /// </summary>
        public UnitOfWork() : this(new ApplicationDbContext())
        {
        }

        /// <summary>
        /// ConnectionString kommt aus den appsettings.json
        /// </summary>
        public UnitOfWork(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
            Sensors = new SensorRepository(dbContext);
            Measurements = new MeasurementRepository(dbContext);
        }

        public ISensorRepository Sensors { get; }
        public IMeasurementRepository Measurements { get; }


        public async Task DeleteDatabaseAsync()
        {
            await DbContext.Database.EnsureDeletedAsync();
        }

        public async Task MigrateDatabaseAsync()
        {
            await DbContext.Database.MigrateAsync();
        }


        public async Task<int> SaveChangesAsync()
        {
            //var entities = _dbContext.ChangeTracker.Entries()
            //    .Where(entity => entity.State == EntityState.Added
            //                     || entity.State == EntityState.Modified)
            //    .Select(e => e.Entity)
            //    .ToArray();  // Geänderte Entities ermitteln
            //foreach (var entity in entities)
            //{
            //    if (entity is IDatabaseValidatableObject)
            //    {     // UnitOfWork injizieren, wenn Interface implementiert ist
            //        var validationContext = new ValidationContext(entity, null, null);
            //        validationContext.InitializeServiceProvider(serviceType => this);

            //        var validationResults = new List<ValidationResult>();
            //        var isValid = Validator.TryValidateObject(entity, validationContext, validationResults,
            //            validateAllProperties: true);
            //        if (!isValid)
            //        {
            //            var memberNames = new List<string>();
            //            List<ValidationException> validationExceptions = new List<ValidationException>();
            //            foreach (ValidationResult validationResult in validationResults)
            //            {
            //                validationExceptions.Add(new ValidationException(validationResult, null, null));
            //                memberNames.AddRange(validationResult.MemberNames);
            //            }

            //            if (validationExceptions.Count == 1)  // eine Validationexception werfen
            //            {
            //                throw validationExceptions.Single();
            //            }
            //            else  // AggregateException mit allen ValidationExceptions als InnerExceptions werfen
            //            {
            //                throw new ValidationException($"Entity validation failed for {string.Join(", ", memberNames)}",
            //                    new AggregateException(validationExceptions));
            //            }
            //        }
            //    }
            //}
            return await DbContext.SaveChangesAsync();
        }


        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    await DbContext.DisposeAsync();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            DbContext?.Dispose();
            GC.SuppressFinalize(this);
        }


        public bool ContextHasChanges()
        {
            return DbContext.ChangeTracker.HasChanges();
        }

        public void Reload(IEntityObject entity)
        {
            DbContext.Entry(entity).Reload();
        }

        public void ClearChangeTracker()
        {
            DbContext.ChangeTracker.Clear();
        }



    }

}
