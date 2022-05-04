using Base.Contracts.Persistence;

using Core.Entities;
using System.Threading.Tasks;

namespace Core.Contracts
{
    public interface IActorRepository : IGenericRepository<Actor>
    {
        Task<Actor[]> GetAsync();
        Actor GetByName(string actorName);
        Task SynchronizeAsync(Actor actor);

    }
}
