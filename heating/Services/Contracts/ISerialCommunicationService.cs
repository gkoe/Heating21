
using System;

namespace Services.Contracts
{
    public interface ISerialCommunicationService
    {
        event EventHandler<string> MessageReceived;

        void StartCommunication();
        void StopCommunication();
        void Send(string message);
    }
}