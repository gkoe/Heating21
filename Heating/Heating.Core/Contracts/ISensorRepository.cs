using System.Threading.Tasks;

using Heating.Core.DataTransferObjects;
using Heating.Core.Entities;

namespace Heating.Core.Contracts
{
    public interface ISensorRepository : IGenericRepository<Sensor>
    {
        Task<Sensor[]> GetOrderedByNameAsync();

        Task<SensorWithMeasurementDto[]> GetWithLastMeasurementAsync();

    }
}
