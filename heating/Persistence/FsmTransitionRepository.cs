using Base.Persistence.Repositories;

using Core.Contracts;
using Core.Entities;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Persistence
{
    public class FsmTransitionRepository : GenericRepository<FsmTransition>, IFsmTransitionRepository
    {
        private ApplicationDbContext DbContext { get; }

        public FsmTransitionRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            DbContext = dbContext;
        }

        public async Task<FsmTransition[]> GetByDay(DateTime day)
        {
            var fsmTransitions = await DbContext
                .FsmTransitions
                .Where(t => t.Time.Date == day.Date)
                .OrderBy(t => t.Time)
                .ToArrayAsync();
            return fsmTransitions;
        }


    }
}
