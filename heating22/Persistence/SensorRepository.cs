
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

        //public async Task UpsertAsync(Sensor stateSensor)
        //{
        //    var dbSensor = await DbContext.Sensors.FirstOrDefaultAsync(s => s.Name == stateSensor.Name);
        //    if (dbSensor == null)
        //    {
        //        dbSensor = new Sensor { Name = stateSensor.Name };
        //        await DbContext.AddAsync(dbSensor);
        //    }
        //    else
        //    {
        //        stateSensor.Id = dbSensor.Id;
        //    }
        //}

        public async Task SynchronizeAsync(Sensor sensor)
        {
            var dbSensor = await DbContext.Sensors.SingleOrDefaultAsync(s => s.Name == sensor.Name);
            if (dbSensor == null)  // Sensor neu anlegen
            {
                sensor.Name = sensor.ItemEnum.ToString();
                await DbContext.Sensors.AddAsync(sensor);
            }
            else
            {
                sensor.Id = dbSensor.Id;
                sensor.Name = dbSensor.Name;
                sensor.Unit = dbSensor.Unit;
                sensor.RowVersion = dbSensor.RowVersion;
            }
            await DbContext.SaveChangesAsync();
        }
    }
}
