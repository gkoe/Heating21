using Microsoft.Extensions.Hosting;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using Serilog;
using Services.Contracts;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Base.ExtensionMethods;
using Services.DataTransferObjects;
using Core.DataTransferObjects;
using Base.Helper;
using Core.Entities;

namespace Services
{
    public class HomematicHttpCommunicationService : IHomematicHttpCommunicationService
    {
        //private const string URL_LIVINGROOM = "http://10.0.0.2:2121/device/HEQ0105664/1/TEMPERATURE/~pv";
        //private const string URL_OUTDOOR = "http://10.0.0.2:2121//device/00185D898B0094/1/ACTUAL_TEMPERATURE/~pv";

        readonly string[] urls = { "http://10.0.0.2:2121/device/HEQ0105664/1/TEMPERATURE/~pv", "http://10.0.0.2:2121//device/00185D898B0094/1/ACTUAL_TEMPERATURE/~pv" };
        readonly ItemEnum[] sensorItems = { ItemEnum.HmoLivingroomFirstFloor, ItemEnum.HmoTemperatureOut };
        private readonly IHttpClientFactory _httpClientFactory;
        private HttpClient _httpClient;
        //private readonly JsonSerializerOptions _options;

        public string UrlFirstFloorLivingRoom { get; init; }
        public string UrlGroundFloorLivingRoom { get; init; }

        public HomematicHttpCommunicationService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            //var appSettingsSection = configuration.GetSection("Communication");
            //UrlFirstFloorLivingRoom = appSettingsSection["UrlFirstFloorLivingRoom"];
            //UrlGroundFloorLivingRoom = appSettingsSection["UrlGroundFloorLivingRoom"];
        }

        public event EventHandler<MeasurementDto> MeasurementReceived;
        private bool StopGetByHttp { get; set; }

        public void StartCommunication()
        {
            Task.Run(() => GetMeasurementsByHttpInLoopAsync());
        }

        private async Task GetMeasurementsByHttpInLoopAsync()
        {
            // temperature_01/state/{"timestamp":1625917023,"value":25.17}
            //{ "sensor": temperature,"time": 2021 - 07 - 17 20:52:50,"value": 24.42 Grad}
            _httpClient = _httpClientFactory.CreateClient();
            Log.Information("HomematicHttpCommunicationService;started");
            while (!StopGetByHttp)
            {
                for (int i = 0; i < urls.Length; i++)
                {
                    try
                    {
                        using var response = await _httpClient.GetAsync(urls[i], HttpCompletionOption.ResponseHeadersRead);
                        //response.EnsureSuccessStatusCode();  //!
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var text = await response.Content.ReadAsStringAsync();
                            var measurement = GetMeasurementFromMessage(text, sensorItems[i]);
                            if (measurement != null)
                            {
                                MeasurementReceived?.Invoke(this, measurement);

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("HomematicHttpCommunicationService;GetHttpMeasurement; Exception: {Exception}", ex.Message);
                    }

                }
                await Task.Delay(10000);
            }
        }

        private  static MeasurementDto GetMeasurementFromMessage(string message, ItemEnum sensorEnum)
        {
            if (RuleEngine.Instance == null || RuleEngine.Instance.StateService == null)
            {
                return null;
            }
            //// 20211023173052
            //// http://10.0.0.2:2121/device/HEQ0105664/1/TEMPERATURE/~pv

            //{
            //    "ts": 1635002915249,
            //          1625917023
            //      "v": 23.4,
            //    "s": 0
            //}
            var response = JsonSerializerExtensions.DeserializeAnonymousType(message, new { ts = 0L, v = 0.0, s = 0 });
            var sensor = RuleEngine.Instance.StateService.GetSensor(sensorEnum);
            if (sensor == null)
            {
                Log.Error($"HomematicHttpCommunicationService;GetMeasurementFromMessage;Sensor {ItemEnum.LivingroomFirstFloor} doesn't exist");
                return null;
            }
            DateTime time = TimeConverters.UnixTimeStampToDateTime(response.ts/1000);
            double? value = response.v;
            if (value != null)
            {
                sensor.AddMeasurement(time, value.Value);
                var measurement = new MeasurementDto
                {
                    SensorId = sensor.Id,
                    SensorName = sensor.ItemName,
                    Time = time,
                    Trend = sensor.Trend,
                    Value = value.Value
                };
                return measurement;
            }
            else
            {
                Log.Error($"HomematicHttpCommunicationService;GetMeasurementFromMessage; Illegal value: {response.v}");
                return null;
            }
        }


        public void StopCommunication()
        {
            StopGetByHttp = true;
        }

    }
}
