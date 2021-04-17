using System.Threading.Tasks;

using Heating.Core.Entities;
using Heating.Core.DataTransferObjects;

namespace Heating.Core.Contracts
{
    public interface IMeasurementRepository : IGenericRepository<Measurement>
    {
        //Task<Measurement[]> GetLast100Async(int sensorId);
    }
}
