using System.Linq;
using System.Threading.Tasks;

using Heating.Core.Contracts;
using Heating.Core.DataTransferObjects;
using Heating.Core.Entities;

using Microsoft.EntityFrameworkCore;

namespace Heating.Persistence
{
    public class SensorRepository : GenericRepository<Sensor>, ISensorRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public SensorRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Sensor[]> GetOrderedByNameAsync()
        {
            var sensors = await _dbContext.Sensors
                .OrderBy(s => s.Name)
                .ToArrayAsync();
            return sensors;
        }

        public async Task<SensorWithMeasurementDto[]> GetWithLastMeasurementAsync()
        {
            var sensors = await _dbContext.Sensors
                         .AsNoTracking()
                         .OrderBy(s => s.Name)
                         .Select(s => new SensorWithMeasurementDto
                         (
                             s.Name,
                             s.Measurements.OrderByDescending(m => m.Time).FirstOrDefault().Time,
                             s.Measurements.OrderByDescending(m => m.Time).FirstOrDefault().Value
                         ))
                         .ToArrayAsync();
            return sensors;
        }

    }
}
