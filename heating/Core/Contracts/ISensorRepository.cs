using Base.Contracts.Persistence;

using Core.Entities;
using System.Threading.Tasks;

namespace Core.Contracts
{
    public interface ISensorRepository : IGenericRepository<Sensor>
    {
        Task<Sensor[]> GetAsync();
        Sensor GetByName(string sensorName);
        Task UpsertAsync(string sensorName);
    }
}
