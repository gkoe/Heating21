using Common.Persistence;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class ApplicationDbContext : CommonApplicationDbContext
    {
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<Measurement> Measurements { get; set; }


        public ApplicationDbContext():base()
        {

        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }


        ///// <summary>
        ///// Für InMemory-DB in UnitTests
        ///// </summary>
        ///// <param name="options"></param>
        //public ApplicationDbContext(DbContextOptions<BaseApplicationDbContext> options) : base(options)
        //{
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<ShotGoals>()
            //    .HasIndex(gt => new { gt.Round, gt.TeamId })
            //    .IsUnique(true);
        }

    }
}
