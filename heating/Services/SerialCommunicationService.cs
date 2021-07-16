using Microsoft.Extensions.Hosting;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using Serilog;
using Services.Contracts;

namespace Services
{
    public class SerialCommunicationService : BackgroundService
    {
        private const string UART_PORT = "COM3"; //"/dev/ttyUSB0"; // "COM7";
        private const int BAUDRATE = 115200;
        SerialPort _serialPort;

        public event EventHandler<string> MessageReceived;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("SerialCommunicationService started");
            _serialPort = new SerialPort(UART_PORT, BAUDRATE) { ReadTimeout = 1500, WriteTimeout = 1500 };
            StringBuilder receivedChars = new StringBuilder();
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
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(100, stoppingToken);
            }
            //return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                Log.Information("SerialCommunicationService, Port is closed!");
                _serialPort.Close();
            }
            return base.StopAsync(cancellationToken);
        }
    }
}
