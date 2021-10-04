using System;
using System.Diagnostics;

using Base.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Base.Persistence
{
    public class BaseApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public BaseApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public BaseApplicationDbContext() : base()
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Session> Sessions { get; set; }


        /// <summary>
        /// Für InMemory-DB in UnitTests
        /// </summary>
        /// <param name="options"></param>
        public BaseApplicationDbContext(DbContextOptions<BaseApplicationDbContext> options) : base(options)
        {
        }

    }
}
