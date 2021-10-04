
using System.Linq;
using System.Threading.Tasks;

using Base.Persistence.Repositories;

using Core.Contracts;
using Core.Entities;

using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class SensorRepository : GenericRepository<Sensor>, ISensorRepository
    {
        private ApplicationDbContext DbContext { get; }

        public SensorRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            DbContext = dbContext;
        }

        public Sensor GetByName(string sensorName)
        {
            return DbContext.Sensors.FirstOrDefault(s => s.Name == sensorName);
        }

        public async Task<Sensor[]> GetAsync()
        {
            var sensors = await DbContext.Sensors
                .OrderBy(s => s.Name)
                .ToArrayAsync();
            return sensors;
        }

        public async Task UpsertAsync(string sensorName)
        {
            var sensor = await DbContext.Sensors.FirstOrDefaultAsync(s => s.Name == sensorName);
            if (sensor == null)
            {
                sensor = new Sensor { Name = sensorName };
                await DbContext.AddAsync(sensor);
            }
        }
    }
}
