
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface IRaspberryIoService
    {
        Task ResetEspAsync();
    }
}