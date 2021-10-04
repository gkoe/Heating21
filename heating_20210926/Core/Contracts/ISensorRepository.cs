using Common.Contracts;
using Core.Entities;
using System.Threading.Tasks;

namespace Core.Contracts
{
    public interface ISensorRepository : IGenericRepository<Sensor>
    {
        Task<Sensor[]> GetAsync();
        Sensor GetByName(string sensorName);
    }
}
