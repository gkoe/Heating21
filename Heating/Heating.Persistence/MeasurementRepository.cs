using System.Linq;
using System.Threading.Tasks;

using Heating.Core.Contracts;
using Heating.Core.DataTransferObjects;
using Heating.Core.Entities;

using Microsoft.EntityFrameworkCore;

namespace Heating.Persistence
{
    public class MeasurementRepository : GenericRepository<Measurement>, IMeasurementRepository
    {
        //private readonly ApplicationDbContext _dbContext;

        public MeasurementRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            //_dbContext = dbContext;
        }

        //public async Task<Measurement[]> GetLast100Async(int sensorId)
        //{
        //    var measurements = await _dbContext
        //        .Measurements
        //        .AsNoTracking()
        //        .Where(m => m.SensorId == sensorId)
        //        .OrderByDescending(m => m.Time)
        //        .Take(100)
        //        .ToArrayAsync();
        //    return measurements;

        //}

    }
}
