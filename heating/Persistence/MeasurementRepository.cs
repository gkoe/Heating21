using System;
using System.Linq;
using System.Threading.Tasks;

using Base.Persistence.Repositories;

using Core.Contracts;
using Core.DataTransferObjects;
using Core.Entities;

using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class MeasurementRepository : GenericRepository<Measurement>, IMeasurementRepository
    {
        private ApplicationDbContext DbContext { get; }

        public MeasurementRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            DbContext = dbContext;
        }

        public async Task<Measurement> GetLastAsync()
        {
            var measurement = await DbContext.Measurements
                .OrderByDescending(m => m.Time)
                .FirstOrDefaultAsync();
            return measurement;
        }

        public async Task<Measurement[]> GetLast100(int sensorId)
        {
            var measurements = await DbContext
                .Measurements
                .Where(m => m.SensorId == sensorId)
                .OrderByDescending(m => m.Time)
                .Take(100)
                .ToArrayAsync();
            return measurements;
        }

        public async Task<Measurement[]> GetByDay(string sensorName, DateTime day)
        {
            var measurements = await DbContext
                .Measurements
                .Where(m => m.Sensor.Name == sensorName && m.Time.Date == day.Date)
                .OrderBy(m => m.Time)
                .ToArrayAsync();
            return measurements;
        }

        public async Task<Measurement> GetLastAsync(string sensorName)
        {
            var measurement = await DbContext
                .Measurements
                .Where(m => m.Sensor.Name == sensorName)
                .OrderByDescending(m => m.Time)
                .FirstOrDefaultAsync();
            return measurement;
        }

        public async Task AddAsync(MeasurementDto measurementDto)
        {
            var measurement = new Measurement
            {
                Id = 0,
                SensorId = measurementDto.SensorId,
                Value = measurementDto.Value,
                Time = measurementDto.Time,
                Trend = measurementDto.Trend
            };
            await DbContext.Measurements.AddAsync(measurement);
        }
    }
}
