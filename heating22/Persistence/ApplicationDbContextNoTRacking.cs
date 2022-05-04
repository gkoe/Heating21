using Base.Persistence;

using Core.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Persistence
{
    public class ApplicationDbContextNoTracking : ApplicationDbContext
    {
        public ApplicationDbContextNoTracking() : base()
        {
        }

        public ApplicationDbContextNoTracking(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        public ApplicationDbContextNoTracking(string connectionString) :base(connectionString)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            this.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }



    }
}
