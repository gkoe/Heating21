using System;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Serilog;
using Services.Contracts;
using Microsoft.Extensions.Configuration;

namespace Services
{
    public class SerialCommunicationService : ISerialCommunicationService
    {
        //private const string UART_PORT = "COM3"; // "/dev/ttyUSB0"; // "COM3";
        private const int BAUDRATE = 115200;
        private SerialPort _serialPort;

        public string SerialPortName { get; }

        public event EventHandler<string> MessageReceived;

        public SerialCommunicationService(IConfiguration configuration)
        {
            var appSettingsSection = configuration.GetSection("Communication");
            SerialPortName = appSettingsSection["SerialPort"];
        }

        public void StartCommunication()
        {
            Log.Information("SerialCommunicationService started");
            _serialPort = new SerialPort(SerialPortName, BAUDRATE) { ReadTimeout = 1500, WriteTimeout = 1500 };
            StringBuilder receivedChars = new StringBuilder();
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
                            MessageReceived?.Invoke(this, receivedChars.ToString());
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
            await Task.Run(() => _serialPort.Write(message + '\n'));
            //_serialPort.WriteLine(message);
            await Task.Delay(100);
        }

        public async Task SetActorAsync(string actorName, double value)
        {
            int intValue = (int)value;
            var message = $"heating/{actorName}/command:{intValue}";
            await SendAsync(message);
            Log.Error($"StateService; SetActorBySerialCommunication; message: {message}");
        }


    }
}
