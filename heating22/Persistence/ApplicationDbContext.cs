using Base.Helper;
using Base.Persistence;

using Core.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Persistence
{
    public class ApplicationDbContext : BaseApplicationDbContext
    {
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Measurement> Measurements { get; set; }
        public DbSet<FsmTransition> FsmTransitions { get; set; }
        public string ConnectionString { get; }

        public ApplicationDbContext() : base()
        {
            ConnectionString = ConfigurationHelper
                .GetConfiguration("DefaultConnection", "ConnectionStrings");
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        public ApplicationDbContext(string connectionString) : base()
        {
            ConnectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

                optionsBuilder.UseSqlite(ConnectionString);
            }
        }

    }
}
