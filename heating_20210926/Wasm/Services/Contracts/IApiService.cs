using System.Threading.Tasks;
using Common.DataTransferObjects;

using Core.Entities;
using Core.DataTransferObjects;

namespace Wasm.Services.Contracts
{
    public interface IApiService
    {

        Task<bool> ChangeSwitchAsync(string name, bool on);
        Task<bool> SetManualOperationAsync(bool on);

        Task ResetEspAsync();
    }
}
