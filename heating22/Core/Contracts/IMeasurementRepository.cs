using Base.Contracts.Persistence;

using Core.DataTransferObjects;
using Core.Entities;

using System;
using System.Threading.Tasks;

namespace Core.Contracts
{
    public interface IMeasurementRepository : IGenericRepository<Measurement>
    {
        Task<Measurement[]> GetLast100(int sensorId);
        Task<Measurement> GetLastAsync(string sensorName);
        Task<Measurement> GetLastAsync();
        Task<Measurement[]> GetByDay(string sensorName, DateTime day);
        Task<MeasurementDto[]> GetFilteredAsync(string itemname);
    }
}
