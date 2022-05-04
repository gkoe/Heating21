using System;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Serilog;
using Services.Contracts;
using Microsoft.Extensions.Configuration;
using Core.DataTransferObjects;
using Base.ExtensionMethods;
using System.Collections.Generic;
using Base.Helper;
using Core.Entities;
using IotServices.Services;

namespace Services
{
    public class SerialCommunicationService : ISerialCommunicationService
    {
        private static readonly Lazy<SerialCommunicationService> lazy = new(() => new SerialCommunicationService());
        public static SerialCommunicationService Instance { get { return lazy.Value; } }

        //private const string UART_PORT = "COM3"; // "/dev/ttyUSB0"; // "COM3";
        private const int BAUDRATE = 115200;
        private SerialPort? _serialPort;

        public string SerialPortName { get; }

        public event EventHandler<MeasurementDto>? MeasurementReceived;

        public SerialCommunicationService()
        {
            SerialPortName = ConfigurationHelper.GetConfiguration("SerialPort", "Communication");
        }

        public void StartCommunication()
        {
            Log.Information("SerialCommunicationService started");
            _serialPort = new SerialPort(SerialPortName, BAUDRATE) { ReadTimeout = 1500, WriteTimeout = 1500 };
            StringBuilder receivedChars = new();
            try
            {
                _serialPort.Open();
                Log.Information("SerialCommunicationService, Port is open!");
                _serialPort.DataReceived += (sender, eventArgs) =>
                {
                    var uart = sender as SerialPort;
                    int charsToRead = _serialPort.BytesToRead;
                    for (int i = 0; i < charsToRead; i++)
                    {
                        var readChar = _serialPort.ReadChar();
                        if (readChar == '\n')
                        {
                            Log.Information($"SerialCommunicationService; data received: {receivedChars}");
                            var measurement = GetMeasurementFromSerialMessage(receivedChars.ToString());
                            if (measurement != null)
                            {
                                MeasurementReceived?.Invoke(this, new MeasurementDto(measurement));
                            }
                            //Console.Write($">>>>>>>>>>>>> {receivedChars}");
                            receivedChars.Clear();
                        }
                        else
                        {
                            receivedChars.Append((char)readChar);
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                Log.Error($"SerialCommunication; Exception: {ex.Message}");
            }
        }

        private static Measurement? GetMeasurementFromSerialMessage(string message)
        {
            //if (RuleEngine.Instance == null || RuleEngine.Instance.StateService == null)
            //{
            //    return null;
            //}

            // heating/OilBurnerSwitch/command:1}
            // temperature_01/state/{"timestamp":1625917023,"value":25.17}
            if (message.Contains("command"))
            {
                return null;
            }
            var startIndex = message.IndexOf('/');
            if (startIndex < 0)
            {
                return null;
            }
            string itemName = message[..startIndex];
            // Test, ob der itemName ein gültiger Sensor oder Actor ist
            Item? item = StateService.Instance.GetSensor(itemName);
            if (item == null)
            {
                item = StateService.Instance.GetActor(itemName);
                if (item == null)
                {
                    Log.Debug($"SerialCommunication; GetMeasurementFromMessage; Item not found: {itemName}");
                    return null;
                }
            }
            startIndex = message.IndexOf('{');
            var payload = message[startIndex..];
            (DateTime time, double? value) = ParseSerialPayload(payload);
            if (value != null)
            {
                var measurement = item.AddMeasurement(time, value.Value);
                return measurement;
            }
            return null;
        }


        public void StopCommunication()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                Log.Information("SerialCommunicationService, Port is closed!");
                _serialPort.Close();
            }
        }

        public async Task SendAsync(string message)
        {
            if (_serialPort!.IsOpen)
            {
                await Task.Run(() => _serialPort.Write(message + '\n'));
                //_serialPort.WriteLine(message);
                await Task.Delay(100);
            }
            else
            {
                Log.Error("NO CONNECTION TO ESP");
            }
        }

        public async Task SetActorAsync(string actorName, double value)
        {
            int intValue = (int)value;
            var message = $"heating/{actorName}/command:{intValue}";
            await SendAsync(message);
            Log.Information($"SerialCommunicationService; SetActorBySerialCommunication; message: {message}");
        }

        private static (DateTime, double?) ParseSerialPayload(string payload)
        {
            var text = payload.RemoveChars("\"{}\\");
            var properties = text.ToString().Split(',');
            var propertyValues = new Dictionary<string, string>();
            foreach (var property in properties)
            {
                var keyValuePairs = property.Split(':');
                propertyValues.Add(keyValuePairs[0].ToLower(), keyValuePairs[1]);
            }
            DateTime time = DateTime.MinValue;
            if (propertyValues.ContainsKey("timestamp"))
            {
                time = DateTimeHelpers.UnixTimeStampToDateTime(double.Parse(propertyValues["timestamp"]));
            }
            double? value = null;
            if (propertyValues.ContainsKey("value"))
            {
                string valueString = propertyValues["value"];
                value = valueString.TryParseToDouble();
                //value = NumberConverters.ParseInvariantDouble(valueString);
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
