
using Common.Helper;

using Core.Entities;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using Services.Contracts;
using Services.DataTransferObjects;
using Services.Hubs;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{

    public class StateService : BackgroundService
    {
        public ConcurrentDictionary<string, SensorWithHistory> Sensors { get; set; } = new ConcurrentDictionary<string, SensorWithHistory>();
        public SerialCommunicationService SerialCommunicationService { get; }
        public IHubContext<MeasurementsHub> MeasurementsHubContext { get; }
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SensorWithHistory GetSensor(string sensorName) => Sensors[sensorName];

        public StateService(IServiceProvider serviceProvider,
            IHubContext<MeasurementsHub> measurementsHubContext)
        {
            //SerialCommunicationService = serialCommunicationService;
            SerialCommunicationService = serviceProvider.GetService<SerialCommunicationService>();
            using (var scope = serviceProvider.CreateScope())
            {
                var x = scope.ServiceProvider.GetService<SerialCommunicationService>();
            }
            MeasurementsHubContext = measurementsHubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("StateService started");
            SerialCommunicationService.MessageReceived += SerialCommunicationService_MessageReceived;
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(100, stoppingToken);
            }
            //return Task.CompletedTask;
        }


        private void SerialCommunicationService_MessageReceived(object sender, string message)
        {
            Log.Information($"message received: {message}");
            AddMeasurementFromSerial(message);
            //MeasurementsHubContext.Clients.All.SendAsync("ReceiveMessage", message);
        }

        public void AddMeasurementFromSerial(string message)
        {
            // temperature_01/state/{"timestamp":1625917023,"value":25.17}
            string sensorName = message.Substring(0, message.IndexOf('/'));
            if (!Sensors.ContainsKey(sensorName))
            {
                Sensors[sensorName] = new SensorWithHistory { SensorName = sensorName };
            }
            var sensor = Sensors[sensorName];
            var payload = message[message.IndexOf('{')..];
            (DateTime time, double? value) = ParseSerialPayload(payload);
            if (value != null)
            {
                sensor.AddMeasurement(time, value.Value);
            }
        }

        private static (DateTime, double?) ParseSerialPayload(string payload)
        {
            var text = payload.RemoveChars("\"{}\\");
            var properties = text.ToString().Split(',');
            Dictionary<string, string> propertyValues = new Dictionary<string, string>();
            foreach (var property in properties)
            {
                var keyValuePairs = property.Split(':');
                propertyValues.Add(keyValuePairs[0].ToLower(), keyValuePairs[1]);
            }
            DateTime time = DateTime.MinValue;
            if (propertyValues.ContainsKey("timestamp"))
            {
                time = TimeConverters.UnixTimeStampToDateTime(double.Parse(propertyValues["timestamp"]));
            }
            double? value = null;
            if (propertyValues.ContainsKey("value"))
            {
                string valueString = propertyValues["value"];
                value = NumberConverters.ParseInvariantDouble(valueString);
                if (value != null)
                {
                    value = value.Value;
                }
                else
                {
                    Log.Error("ParseSerialPayload, double.TryParse: '{ValueString}', Length: {Length}, FormatException",
                            valueString, valueString.Length);
                }
            }
            return (time, value);
        }

    }



}
