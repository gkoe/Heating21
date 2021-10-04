using Base.Persistence.Repositories;

using Core.Contracts;
using Core.Entities;

namespace Persistence
{
    public class FsmTransitionRepository : GenericRepository<FsmTransition>, IFsmTransitionRepository
    {
        //private ApplicationDbContext DbContext { get; }

        public FsmTransitionRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            //DbContext = dbContext;
        }
    }
}
