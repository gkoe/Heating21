using Serilog;
using Services.Contracts;
using Base.ExtensionMethods;
using Core.DataTransferObjects;
using Core.Entities;
using Base.Helper;
using IotServices.Services;

namespace Services
{
    public class EspHttpCommunicationService : IEspHttpCommunicationService
    {
        private static readonly Lazy<EspHttpCommunicationService> lazy = new(() => new EspHttpCommunicationService());
        public static EspHttpCommunicationService Instance { get { return lazy.Value; } }

        //private const string URL_LIVINGROOM = "http://192.168.0.52/sensor?temperature";
        //private const string URL_LIVINGROOM = "http://192.168.0.23/sensor?temperature";
        private IHttpClientFactory? _httpClientFactory;
        //private readonly JsonSerializerOptions _options;

        public string UrlFirstFloorLivingRoom { get; private set; } = string.Empty;
        public string UrlGroundFloorLivingRoom { get; private set; } = string.Empty;


        public void Init(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            var configuration = ConfigurationHelper.GetConfiguration();
            var appSettingsSection = configuration.GetSection("Communication");
            UrlFirstFloorLivingRoom = appSettingsSection["UrlFirstFloorLivingRoom"];
            UrlGroundFloorLivingRoom = appSettingsSection["UrlGroundFloorLivingRoom"];
        }

        public event EventHandler<MeasurementDto>? MeasurementReceived;
        private bool StopGetByHttp { get; set; }

        public void StartCommunication()
        {
            Task.Run(() => GetMeasurementsByHttpInLoopAsync());
        }

        private async Task GetMeasurementsByHttpInLoopAsync()
        {
            // temperature_01/state/{"timestamp":1625917023,"value":25.17}
            //{ "sensor": temperature,"time": 2021 - 07 - 17 20:52:50,"value": 24.42 Grad}
            if (_httpClientFactory == null)
            {
                Log.Error("EspHttpCommunicationService; GetMeasurementsByHttpInLoopAsync; HttpClientFactory is null");
                return;
            }
            var httpClient = _httpClientFactory.CreateClient();
            Log.Information("EspHttpCommunicationService;started");
            while (!StopGetByHttp)
            {
                try
                {
                    using var response = await httpClient.GetAsync(UrlFirstFloorLivingRoom, HttpCompletionOption.ResponseHeadersRead);
                    //response.EnsureSuccessStatusCode();  //!
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var text = await response.Content.ReadAsStringAsync();
                        var measurement = GetMeasurementFromMessage(text);
                        if (measurement != null)
                        {
                            MeasurementReceived?.Invoke(this, new MeasurementDto(measurement));

                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("EspHttpCommunicationService;GetHttpMeasurement; Exception: {Exception}", ex.Message);
                }
                await Task.Delay(10000);
            }
        }

        private  static Measurement? GetMeasurementFromMessage(string message)
        {
            //if (RuleEngine.Instance == null || RuleEngine.Instance.StateService == null)
            //{
            //    return null;
            //}
            //{ "sensor": temperature,"time": 2021 - 07 - 17 20:52:50,"value": 24.42 Grad}
            message = message.RemoveChars(" ");
            //string sensorName = "LivingroomFirstFloor";
            var sensor = StateService.Instance.GetSensor(ItemEnum.LivingroomFirstFloor);
            if (sensor == null)
            {
                Log.Error($"EspHttpCommunicationService;GetMeasurementFromMessage;Sensor {sensor} doesn't exist");
                return null;
            }
            var startPos = message.IndexOf("time") + 6;
            var endPos = message.IndexOf("value") - 2;
            var length = endPos - startPos;
            if (length < 18)
            {
                Log.Information($"EspHttpCommunicationService;GetMeasurementFromMessage; parse time; Illegal length: {length}");
                return null;
            }
            string timeString = message.Substring(startPos, 10) + " " + message.Substring(startPos + 10, 8);
            DateTime time = DateTime.Parse(timeString);
            startPos = message.IndexOf("value") + 7;
            endPos = message.IndexOf("Grad");
            length = endPos - startPos;
            if (length < 1)
            {
                Log.Information($"EspHttpCommunicationService;GetMeasurementFromMessage; parse value; Illegal length: {length}");
                return null;
            }
            string valueString = message.Substring(startPos, length);
            //double? value = NumberConverters.ParseInvariantDouble(valueString);
            double? value = valueString.TryParseToDouble();
            if (value != null)
            {
                var measurement = sensor.AddMeasurement(time, value.Value);
                return measurement;
            }
            else
            {
                Log.Error($"EspHttpCommunicationService;GetMeasurementFromMessage; Illegal valueString: {valueString}");
                return null;
            }
        }


        public void StopCommunication()
        {
            StopGetByHttp = true;
        }

    }
}
