
using Core.DataTransferObjects;

using System;

namespace Services.Contracts
{
    public interface IEspHttpCommunicationService
    {
        event EventHandler<MeasurementDto> MeasurementReceived;

        void StopCommunication();
        void StartCommunication();
    }
}