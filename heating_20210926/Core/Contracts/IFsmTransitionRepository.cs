using Common.Contracts;
using Core.Entities;
using System.Threading.Tasks;

namespace Core.Contracts
{
    public interface IFsmTransitionRepository : IGenericRepository<FsmTransition>
    {
    }
}
