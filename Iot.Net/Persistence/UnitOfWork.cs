using System.Configuration;

using Common.Persistence;

using Core.Contracts;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Persistence
{
    public class UnitOfWork : CommonUnitOfWork, IUnitOfWork
    {
        //private readonly IConfiguration _configuration;

        public ApplicationDbContext ApplicationDbContext => BaseApplicationDbContext as ApplicationDbContext;

        public UnitOfWork(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
            //_configuration = configuration;
            //PupilRepository = new PupilRepository(_dbContext);
        }

        //DbContextOptionsBuilder CreateOptions()
        //{
        //    var options = new DbContextOptionsBuilder();
        //    options.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
        //    return options;
        //}

        //public IPupilRepository PupilRepository { get; }


    }
}
