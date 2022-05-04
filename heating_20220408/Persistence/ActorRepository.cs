
using System.Linq;
using System.Threading.Tasks;

using Base.Persistence.Repositories;

using Core.Contracts;
using Core.Entities;

using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class ActorRepository : GenericRepository<Actor>, IActorRepository
    {
        private ApplicationDbContext DbContext { get; }

        public ActorRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            DbContext = dbContext;
        }

        public Actor GetByName(string actorName)
        {
            return DbContext.Actors.FirstOrDefault(s => s.Name == actorName);
        }

        public async Task<Actor[]> GetAsync()
        {
            var actors = await DbContext.Actors
                .OrderBy(s => s.Name)
                .ToArrayAsync();
            return actors;
        }

        public async Task UpsertAsync(Actor stateActor)
        {
            var dbActor = await DbContext.Actors.FirstOrDefaultAsync(s => s.Name == stateActor.Name);
            if (dbActor == null)
            {
                dbActor = new Actor { Name = stateActor.Name };
                await DbContext.AddAsync(dbActor);
            }
            else
            {
                stateActor.Id = dbActor.Id;
            }
        }
    }
}
