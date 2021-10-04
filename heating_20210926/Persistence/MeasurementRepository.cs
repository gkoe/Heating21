using System.Linq;
using System.Threading.Tasks;

using Common.Persistence.Repositories;

using Core.Contracts;
using Core.Entities;

using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class MeasurementRepository : GenericRepository<Measurement>, IMeasurementRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public MeasurementRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Measurement> GetLastAsync()
        {
            var measurement = await _dbContext.Measurements
                .OrderByDescending(m => m.Time)
                .FirstOrDefaultAsync();
            return measurement;
        }

        public async Task<Measurement[]> GetLast100(int sensorId)
        {
            var measurements = await _dbContext
                .Measurements
                .Where(m => m.SensorId == sensorId)
                .OrderByDescending(m => m.Time)
                .Take(100)
                .ToArrayAsync();
            return measurements;
        }

        public async Task<Measurement> GetLastAsync(string sensorName)
        {
            var measurement = await _dbContext
                .Measurements
                .Where(m => m.Sensor.Name == sensorName)
                .OrderByDescending(m => m.Time)
                .FirstOrDefaultAsync();
            return measurement;
        }
    }
}
