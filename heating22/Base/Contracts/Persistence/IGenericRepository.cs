using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Base.Contracts.Entities;

using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Base.Contracts.Persistence
{
    public interface IGenericRepository<TEntity> where TEntity : class, IEntityObject, new()
    {
        /// <summary>
        ///  Liefert eine Menge von Entities zurück. Diese kann optional
        ///  gefiltert und/oder sortiert sein.
        ///  Weiters werden bei Bedarf abhängige Entitäten mitgeladen (eager loading).
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="includeProperties">Liste der einzubindenden Entities</param>
        /// <returns></returns>
        Task<TEntity[]> GetAsync(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null);

        /// <summary>
        ///     Eindeutige Entität oder null zurückliefern
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TEntity> GetByIdAsync(int id);

        Task<bool> ExistsAsync(int id);


        /// <summary>
        /// Fügt neue Identität zum Datenbestand hinzu
        /// </summary>
        /// <param name="entity"></param>
        Task<EntityEntry<TEntity>> AddAsync(TEntity entity);

        /// <summary>
        ///     Liste von Entities einfügen
        /// </summary>
        /// <param name="entities"></param>
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Entity wird in die vom DbContext verwaltete Datenmenge als
        /// geändert hinzugefügt.
        /// </summary>
        /// <param name="entity"></param>
        void AttachAsModified(TEntity entity);

        /// <summary>
        ///     Entität per primary key löschen
        /// </summary>
        /// <param name="id"></param>
        bool Remove(int id);

        /// <summary>
        ///     Übergebene Entität löschen.
        /// </summary>
        /// <param name="entityToDelete"></param>
        void Remove(TEntity entityToDelete);

        ///// <summary>
        /////     Entität aktualisieren
        ///// </summary>
        ///// <param name="entityToUpdate"></param>
        //void Update(TEntity entityToUpdate);

        /// <summary>
        ///     Generisches CountAsync mit Filtermöglichkeit. Sind vom Filter
        ///     Navigationproperties betroffen, können diese per eager-loading
        ///     geladen werden.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="includeProperties"></param>
        /// <returns></returns>
        Task<int> CountAsync(Expression<Func<TEntity, bool>> filter = null,
            params string[] includeProperties);

        bool HasChanges();
    }
}