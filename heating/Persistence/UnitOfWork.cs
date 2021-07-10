using Common.Persistence;
using Core.Contracts;

namespace Persistence
{
    public class UnitOfWork : CommonUnitOfWork, IUnitOfWork
    {
        //private readonly IConfiguration _configuration;

        public ApplicationDbContext ApplicationDbContext => BaseApplicationDbContext as ApplicationDbContext;

        public UnitOfWork(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
            Sensors = new SensorRepository(applicationDbContext);
            Measurements = new MeasurementRepository(applicationDbContext);
        }

        public ISensorRepository Sensors { get; }
        public IMeasurementRepository Measurements { get; }


        //DbContextOptionsBuilder CreateOptions()
        //{
        //    var options = new DbContextOptionsBuilder();
        //    options.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
        //    return options;
        //}

        //public IPupilRepository PupilRepository { get; }


    }
}
