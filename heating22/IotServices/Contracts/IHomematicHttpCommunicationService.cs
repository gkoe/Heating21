
using Core.DataTransferObjects;

using System;
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface IHomematicHttpCommunicationService
    {
        event EventHandler<MeasurementDto> MeasurementReceived;

        void StopCommunication();
        void StartCommunication();
        Task SetTargetTemperatureAsync(double temperature);
    }
}