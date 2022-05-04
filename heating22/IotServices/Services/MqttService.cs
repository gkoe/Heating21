using System.Text;

using Base.Helper;

using IotServices.Contracts;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Options;

using Serilog;

namespace IotServices.Services
{
    public class MqttService : IMqttService
    {

        private static readonly Lazy<MqttService> lazy = new(() => new MqttService());
        public static MqttService Instance { get { return lazy.Value; } }
        private MqttService() { }

        public IMqttClient? MqttClient { get; private set; }

        public IMqttClientOptions? MqttClientOptions { get; private set; }
        private readonly string ClientId = $"MqttService_{Guid.NewGuid()}";

        public event EventHandler<MqttMessage>? MessageReceived;


        /// <summary>
        /// Mit Mqttbroker verbinden und einen Subscriber anmelden.
        /// Rückgabe der Messages über Callback
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <param name="port"></param>
        /// <param name="mqttUser"></param>
        /// <param name="mqttPassword"></param>
        /// <param name="receivedMqttMessageHandler"></param>
        public async Task InitAsync()
        {
            string broker = ConfigurationHelper.GetConfiguration("Broker", "Mqtt");
            int port = int.Parse(ConfigurationHelper.GetConfiguration("Port", "Mqtt"));
            string user = ConfigurationHelper.GetConfiguration("User", "Mqtt");
            string password = ConfigurationHelper.GetConfiguration("Password", "Mqtt");

            // Create a new MQTT client.
            var factory = new MqttFactory();
            MqttClient = factory.CreateMqttClient();

            #region Create TCP based options using the builder
            MqttClientOptions = new MqttClientOptionsBuilder()
                .WithClientId(ClientId)
                .WithTcpServer(broker, port)
                .WithCredentials(user, password)
                .WithTls(new MqttClientOptionsBuilderTlsParameters
                {
                    AllowUntrustedCertificates = true,
                    IgnoreCertificateChainErrors = true,
                    IgnoreCertificateRevocationErrors = true,
                    UseTls = true,
                    SslProtocol = System.Security.Authentication.SslProtocols.Tls12  //  neu mit MQTTnet 3.0.15
                })
                .WithCleanSession()
                //.WithCommunicationTimeout(TimeSpan.FromSeconds(2))
                .Build();
            #endregion

            MqttClient.UseConnectedHandler(async e =>
            {
                Log.Information("Mqtt connected");
                // Subscribe to all topics
                await MqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("#").Build());
                Log.Information("all topics subscribed");
            });
            MqttClient.UseApplicationMessageReceivedHandler(mqttEvent =>
            {
                string topic = mqttEvent.ApplicationMessage.Topic;
                string payload = Encoding.UTF8.GetString(mqttEvent.ApplicationMessage.Payload);
                var qos = mqttEvent.ApplicationMessage.QualityOfServiceLevel;
                var retained = mqttEvent.ApplicationMessage.Retain;
                if (retained)
                {
                    return;
                }
                Log.Information("Mqtt message received, topic: {topic}, payload: {payload}",
                    topic, payload);
                MessageReceived?.Invoke(this, new MqttMessage(topic, payload, (int)qos, retained));
            });
            MqttClient.UseDisconnectedHandler(async e =>
            {
                Log.Error("Mqtt disconnected, {Reasoncode}", e.Reason);
                // Reconnect
                await MqttClient.ConnectAsync(MqttClientOptions, CancellationToken.None); // Since 3.0.5 with CancellationToken
            });
            try  // Connect with broker
            {
               var result = await MqttClient.ConnectAsync(MqttClientOptions, CancellationToken.None); // Since 3.0.5 with CancellationToken
                if (result.ResultCode != MqttClientConnectResultCode.Success)
                {
                    Log.Error("MqttService, Init, ConnectAsync, ErrorCode: {MqttResultcode}", result.ResultCode);
                }
            }
            catch (Exception ex)
            {
                Log.Error("MqttService, Init, Exception: {Exception}", ex.Message);
                StringBuilder innerExceptionMessages = new();
                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    innerExceptionMessages.Append(innerEx.Message);
                    innerExceptionMessages.Append('*');
                    innerEx = innerEx.InnerException;
                }
                Log.Error("MqttService, Init, InnerExceptions: {InnerException}", innerExceptionMessages.ToString());
            }
            Log.Information("MqttService; Init(); MqttClientId: {MqttClientId} ", ClientId);
        }

        public async Task<bool> Publish(string topic, string value)
        {
            if (MqttClient == null ||  MqttClient.IsConnected == false)
            {
                Log.Error("MqttService, Publish, MqttClient not connected");
                return false;
            }

            var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(value)
                    .WithRetainFlag()
                    .Build();
            await MqttClient.PublishAsync(message);
            return true;
        }


    }
}
