﻿
using Common.Helper;

using Core.DataTransferObjects;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

using Services.Contracts;
using Services.DataTransferObjects;
using Services.Hubs;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services
{

    public class StateService : IStateService
    {
        ConcurrentDictionary<string, SensorWithHistory> Sensors { get; set; } = new ConcurrentDictionary<string, SensorWithHistory>();

        protected ISerialCommunicationService SerialCommunicationService { get; private set; }
        protected IHttpCommunicationService HttpCommunicationService { get; private set; }
        IHubContext<MeasurementsHub> MeasurementsHubContext { get; }

        public SensorWithHistory GetSensor(string sensorName) => Sensors[sensorName];

        public event EventHandler<MeasurementDto> NewMeasurement;

        public StateService(IHubContext<MeasurementsHub> measurementsHubContext)
        {
            MeasurementsHubContext = measurementsHubContext;
        }

        public void Init(ISerialCommunicationService serialCommunicationService, IHttpCommunicationService httpCommunicationService)
        {
            Log.Information("StateService started");
            //SerialCommunicationService = serialCommunicationService;
            SerialCommunicationService = serialCommunicationService;
            SerialCommunicationService.MessageReceived += SerialCommunicationService_MessageReceived;
            SerialCommunicationService.StartCommunication();
            HttpCommunicationService = httpCommunicationService;
            HttpCommunicationService.MeasurementReceived += HttpCommunicationService_MeasurementReceived;
            HttpCommunicationService.StartCommunication();
        }

        private async void HttpCommunicationService_MeasurementReceived(object sender, string message)
        {
            Log.Information($"StateService; message received from http: {message}");
            await AddMeasurementFromHttpAsync(message);
            await Task.CompletedTask;
        }

        private async void SerialCommunicationService_MessageReceived(object sender, string message)
        {
            Log.Information($"StateService; message received from serial: {message}");
            await AddMeasurementFromSerialAsync(message);
            // await Clients.All.SendAsync("ReceiveMessage", user, message);
            // MeasurementsHubContext.Clients.All.SendAsync("ReceiveMessage", "StateService", message);
        }

        private async Task AddMeasurementFromHttpAsync(string message)
        {
            //{ "sensor": temperature,"time": 2021 - 07 - 17 20:52:50,"value": 24.42 Grad}
            message = message.RemoveChars(" ");
            int startPos = message.IndexOf(':') + 1;
            string sensorName = message[startPos..message.IndexOf(',')];
            if (!Sensors.ContainsKey(sensorName))
            {
                Sensors[sensorName] = new SensorWithHistory { SensorName = sensorName };
            }
            var sensor = Sensors[sensorName];
            startPos = message.IndexOf("time") + 6;
            var endPos = message.IndexOf("value")-2;
            var length = endPos - startPos;
            if (length < 18)
            {
                Log.Error($"AddMeasurementFromHttpAsync; parse time; Illegal length: {length}");
                return;
            }
            string timeString = message.Substring(startPos, 10)+" "+ message.Substring(startPos+10, 8);
            DateTime time = DateTime.Parse(timeString);
            startPos = message.IndexOf("value") + 7;
            endPos = message.IndexOf("Grad");
            length = endPos - startPos;
            if (length < 1)
            {
                Log.Error($"AddMeasurementFromHttpAsync; parse value; Illegal length: {length}");
                return;
            }
            string valueString = message.Substring(startPos, length);
            double? value = NumberConverters.ParseInvariantDouble(valueString);
            if (value != null)
            {
                sensor.AddMeasurement(time, value.Value);
                var measurement = new MeasurementDto
                {
                    SensorName = sensorName,
                    Time = time,
                    Trend = sensor.Trend,
                    Value = value.Value
                };
                NewMeasurement?.Invoke(this, measurement);
                await MeasurementsHubContext.Clients.All.SendAsync("ReceiveMeasurement", measurement);
            }
            else
            {
                Log.Error($"AddMeasurementFromHttpAsync; Illegal valueString: {valueString}");
            }
        }

        private async Task AddMeasurementFromSerialAsync(string message)
        {
            // temperature_01/state/{"timestamp":1625917023,"value":25.17}
            var startIndex = message.IndexOf('/');
            if (startIndex < 0)
            {
                return;
            }
            string sensorName = message.Substring(0, startIndex);
            if (!Sensors.ContainsKey(sensorName))
            {
                Sensors[sensorName] = new SensorWithHistory { SensorName = sensorName };
            }
            var sensor = Sensors[sensorName];
            startIndex = message.IndexOf('{');
            var payload = message[startIndex..];
            (DateTime time, double? value) = ParseSerialPayload(payload);
            if (value != null)
            {
                sensor.AddMeasurement(time, value.Value);
                var measurement = new MeasurementDto
                {
                    SensorName = sensorName,
                    Time = time,
                    Trend = sensor.Trend,
                    Value = value.Value
                };
                NewMeasurement?.Invoke(this, measurement);
                Log.Information("Send measurement by SignalR: {Name} {Time} {Trend} {Value}", measurement.SensorName, measurement.Time, measurement.Trend, measurement.Value);
                await MeasurementsHubContext.Clients.All.SendAsync("ReceiveMeasurement", measurement);
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

    public class HttpMeasurementDto
    {
        public string Sensor { get; set; }
        public DateTime Time { get; set; }
        public double Value { get; set; }

    }



}
