using Base.Contracts.Persistence;

using Core.Entities;

namespace Core.Contracts
{
    public interface IFsmTransitionRepository : IGenericRepository<FsmTransition>
    {
    }
}
