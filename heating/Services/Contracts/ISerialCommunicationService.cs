
using Services.DataTransferObjects;

using System;
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface ISerialCommunicationService
    {
        event EventHandler<string> MessageReceived;

        void StartCommunication();
        void StopCommunication();
        Task SendAsync(string message);
        public Task SetActorAsync(string actor, double value);
    }
}