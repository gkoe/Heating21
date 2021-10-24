
using Core.DataTransferObjects;

using System;

namespace Services.Contracts
{
    public interface IHomematicHttpCommunicationService
    {
        event EventHandler<MeasurementDto> MeasurementReceived;

        void StopCommunication();
        void StartCommunication();
    }
}