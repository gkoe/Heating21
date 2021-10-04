using Base.Contracts.Persistence;

using Core.DataTransferObjects;
using Core.Entities;
using System.Threading.Tasks;

namespace Core.Contracts
{
    public interface IMeasurementRepository : IGenericRepository<Measurement>
    {
        Task<Measurement[]> GetLast100(int sensorId);
        Task<Measurement> GetLastAsync(string sensorName);
        Task<Measurement> GetLastAsync();
        Task AddAsync(MeasurementDto measurementDto);
    }
}
