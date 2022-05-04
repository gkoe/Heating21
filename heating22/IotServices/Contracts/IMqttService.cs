
using System;
using System.Threading.Tasks;

namespace IotServices.Contracts
{
    public record MqttMessage(string Topic, string Payload, int Qos, bool Retained);

    public interface IMqttService
    {
        Task InitAsync();
        public event EventHandler<MqttMessage>? MessageReceived;

    }
}