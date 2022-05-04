using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Base.Contracts.Entities;
using Base.Contracts.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Base.Persistence.Repositories
{
    /// <summary>
    /// Generische Zugriffsmethoden für eine Entität
    /// Werden spezielle Zugriffsmethoden benötigt, wird eine spezielle
    /// abgeleitete Klasse erstellt.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class, IEntityObject, new()
    {
        private readonly DbSet<TEntity> _dbSet; // Set der entsprechenden Entität im Context

        public GenericRepository(DbContext context)
        {
            Context = context;
            _dbSet = context.Set<TEntity>();
        }

        public DbContext Context { get; }

        /// <summary>
        /// Liefert eine Menge von Entities zurück. Diese kann optional
        /// gefiltert und/oder sortiert sein.
        /// Weiters werden bei Bedarf abhängige Entitäten mitgeladen (eager loading).
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public virtual async Task<TEntity[]> GetAsync(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null)
        {
            IQueryable<TEntity> query = _dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (orderBy != null)
            {
                return await orderBy(query).ToArrayAsync();
            }
            return await query.ToArrayAsync().ConfigureAwait(false);
        }

        /// <summary>
        ///  Eindeutige Entität oder null zurückliefern
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TEntity> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbSet.AnyAsync(e => e.Id == id);
        }

        public async Task<EntityEntry<TEntity>> AddAsync(TEntity entity)
        {
            return await _dbSet.AddAsync(entity);
        }

        /// <summary>
        /// Liste von Entities in den Kontext übernehmen.
        /// Enormer Performancegewinn im Vergleich zum Einfügen einzelner Sätze
        /// </summary>
        /// <param name="entities"></param>
        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        /// <summary>
        /// Entity wird in die vom DbContext verwaltete Datenmenge als
        /// geändert hinzugefügt.
        /// </summary>
        /// <param name="entity"></param>
        public void AttachAsModified(TEntity entity)
        {
            Context.Attach(entity).State = EntityState.Modified;
        }

        /// <summary>
        ///     Entität per primary key löschen
        /// </summary>
        /// <param name="id"></param>
        public bool Remove(int id)
        {
            TEntity entityToDelete = _dbSet.Find(id);
            if (entityToDelete != null)
            {
                Remove(entityToDelete);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        ///  Übergebene Entität löschen. Ist sie nicht im Context verwaltet,
        ///  vorher dem Context zur Verwaltung übergeben.
        /// </summary>
        /// <param name="entityToRemove"></param>
        public void Remove(TEntity entityToRemove)
        {
            if (Context.Entry(entityToRemove).State == EntityState.Detached)
            {
                _dbSet.Attach(entityToRemove);
            }
            _dbSet.Remove(entityToRemove);
        }

        ///// <summary>
        /////     Entität aktualisieren
        ///// </summary>
        ///// <param name="entityToUpdate"></param>
        //public void Update(TEntity entityToUpdate)
        //{
        //    //Prüfen ob Entität bereits im aktuellen Context vorhanden (falls ja, muss vorher Detach auf Entität durchgeführt werden,
        //    //um Attach ausführen zu können)
        //    TEntity localEntity = _dbSet.Local.FirstOrDefault(x => x.Id == entityToUpdate.Id);
        //    if (localEntity != null)
        //    {
        //        if (Context.Entry(entityToUpdate).State == EntityState.Added)
        //        {
        //            throw new DbUpdateException("Update performed on inserted but not commited dataset", default(Exception));
        //        }
        //        Context.Entry(localEntity).State = EntityState.Added;
        //        _dbSet.Local.Remove(localEntity);
        //    }
        //    _dbSet.Attach(entityToUpdate);
        //    Context.Entry(entityToUpdate).State = EntityState.Modified;
        //    //Context.Update(entityToUpdate);
        //}

        /// <summary>
        ///     Generisches CountAsync mit Filtermöglichkeit. Sind vom Filter
        ///     Navigationproperties betroffen, können diese per eager-loading
        ///     geladen werden.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> filter = null,
            params string[] includeProperties)
        {
            IQueryable<TEntity> query = _dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            // alle gewünschten abhängigen Entitäten mitladen
            foreach (string includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return await query.CountAsync();
        }
        public bool HasChanges()
        {
            return Context.ChangeTracker.HasChanges();
        }
    }
}
