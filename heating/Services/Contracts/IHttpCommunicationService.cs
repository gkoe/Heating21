
using System;

namespace Services.Contracts
{
    public interface IHttpCommunicationService
    {
        event EventHandler<string> MeasurementReceived;

        void StopCommunication();
        void StartCommunication();
    }
}