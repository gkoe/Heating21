
using System.Linq;
using System.Threading.Tasks;

using Common.Persistence.Repositories;

using Core.Contracts;
using Core.Entities;

using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class SensorRepository : GenericRepository<Sensor>, ISensorRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public SensorRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public Sensor GetByName(string sensorName)
        {
            return _dbContext.Sensors.FirstOrDefault(s => s.Name == sensorName);
        }

        public async Task<Sensor[]> GetAsync()
        {
            var sensors = await _dbContext.Sensors
                .OrderBy(s => s.Name)
                .ToArrayAsync();
            return sensors;
        }
    }
}
