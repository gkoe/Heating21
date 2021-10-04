using System;
using System.Diagnostics;

using Common.Persistence.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Common.Persistence
{
    public class CommonApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public CommonApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public CommonApplicationDbContext() : base()
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Session> Sessions { get; set; }


        /// <summary>
        /// Für InMemory-DB in UnitTests
        /// </summary>
        /// <param name="options"></param>
        //public BaseApplicationDbContext(DbContextOptions<BaseApplicationDbContext> options) : base(options)
        //{
        //}

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        var builder = new ConfigurationBuilder()
        //            .SetBasePath(Environment.CurrentDirectory)
        //            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        //        var configuration = builder.Build();
        //        Debug.Write(configuration.ToString());
        //        string connectionString = configuration["ConnectionStrings:DefaultConnection"];
        //        optionsBuilder.UseSqlServer(connectionString);
        //        //optionsBuilder.UseLoggerFactory(GetLoggerFactory());
        //    }
        //}

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //    //modelBuilder.Entity<ShotGoals>()
        //    //    .HasIndex(gt => new { gt.Round, gt.TeamId })
        //    //    .IsUnique(true);
        //}

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    //modelBuilder.Entity<ShotGoals>()
        //    //    .HasIndex(gt => new { gt.Round, gt.TeamId })
        //    //    .IsUnique(true);
        //}


        ///// <summary>
        ///// NuGet: Microsoft.Extensions.Logging.Console
        ///// </summary>
        ///// <returns></returns>
        //private ILoggerFactory GetLoggerFactory()
        //{
        //    IServiceCollection serviceCollection = new ServiceCollection();
        //    serviceCollection.AddLogging(builder =>
        //        builder.AddConsole()
        //            .AddFilter(DbLoggerCategory.Database.Command.Name,
        //                LogLevel.Information));
        //    return serviceCollection.BuildServiceProvider()
        //        .GetService<ILoggerFactory>();
        //}

    }
}
