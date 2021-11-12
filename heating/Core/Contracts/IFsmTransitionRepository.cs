using Base.Contracts.Persistence;

using Core.Entities;

using System;
using System.Threading.Tasks;

namespace Core.Contracts
{
    public interface IFsmTransitionRepository : IGenericRepository<FsmTransition>
    {
        Task<FsmTransition[]> GetByDay(DateTime day);

    }
}
