
using System;
using System.Threading.Tasks;
using Serilog;
using Services.Contracts;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Base.ExtensionMethods;
using Core.DataTransferObjects;
using Base.Helper;
using Core.Entities;
using Newtonsoft.Json;
using System.Text;
using IotServices.Services;

namespace Services
{
    public class HomematicHttpCommunicationService : IHomematicHttpCommunicationService
    {
        private static readonly Lazy<HomematicHttpCommunicationService> lazy = new(() => new HomematicHttpCommunicationService());
        public static HomematicHttpCommunicationService Instance { get { return lazy.Value; } }

        //private const string URL_LIVINGROOM = "http://10.0.0.2:2121/device/HEQ0105664/1/TEMPERATURE/~pv";
        private const string URL_OUTDOOR = "http://10.0.0.2:2121//device/00185D898B0094/1/ACTUAL_TEMPERATURE/~pv";
        private const string URL_THERMOSTAT_LIVINGROOM_ACT = "http://10.0.0.2:2121/device/00265D899A9F7C/1/ACTUAL_TEMPERATURE/~pv";
        private const string URL_THERMOSTAT_LIVINGROOM_SET = "http://10.0.0.2:2121/device/00265D899A9F7C/1/SET_POINT_TEMPERATURE/~pv";

        //readonly string[] urls = { "http://10.0.0.2:2121/device/HEQ0105664/1/TEMPERATURE/~pv", "http://10.0.0.2:2121//device/00185D898B0094/1/ACTUAL_TEMPERATURE/~pv" };
        readonly string[] urls = { URL_OUTDOOR, URL_THERMOSTAT_LIVINGROOM_ACT, URL_THERMOSTAT_LIVINGROOM_SET};
        readonly ItemEnum[] sensorItems = { ItemEnum.HmoTemperatureOut, ItemEnum.HmoLivingroomFirstFloor, ItemEnum.HmoLivingroomFirstFloorSet };

        public HttpClient? HttpClient { get; private set; }
        //private readonly JsonSerializerOptions _options;

        public string UrlFirstFloorLivingRoom { get; init; } = String.Empty;
        public string UrlGroundFloorLivingRoom { get; init; } = String.Empty;

        public void Init(IHttpClientFactory httpClientFactory)
        {
            HttpClient = httpClientFactory.CreateClient();
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
            Log.Information("HomematicHttpCommunicationService;started");
            while (!StopGetByHttp)
            {
                for (int i = 0; i < urls.Length; i++)
                {
                    try
                    {
                        using var response = await HttpClient!.GetAsync(urls[i], HttpCompletionOption.ResponseHeadersRead);
                        //response.EnsureSuccessStatusCode();  //!
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var text = await response.Content.ReadAsStringAsync();
                            var measurement = GetMeasurementFromMessage(text, sensorItems[i]);
                            if (measurement != null)
                            {
                                MeasurementReceived?.Invoke(this, new MeasurementDto(measurement));

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

        private  static Measurement? GetMeasurementFromMessage(string message, ItemEnum sensorName)
        {
            //if (RuleEngine.Instance == null || RuleEngine.Instance.StateService == null)
            //{
            //    return null;
            //}
            //// 20211023173052
            //// http://10.0.0.2:2121/device/HEQ0105664/1/TEMPERATURE/~pv

            //{
            //    "ts": 1635002915249,
            //          1625917023
            //      "v": 23.4,
            //    "s": 0
            //}
            var response = JsonSerializerExtensions.DeserializeAnonymousType(message, new { ts = 0L, v = 0.0, s = 0 });
            var sensor = StateService.Instance.GetSensor(sensorName);
            if (sensor == null)
            {
                Log.Error($"HomematicHttpCommunicationService;GetMeasurementFromMessage;Sensor {sensorName} doesn't exist");
                return null;
            }
            DateTime time = DateTimeHelpers.UnixTimeStampToDateTime(response.ts/1000);
            double? value = response.v;
            if (value != null)
            {
                var measurement = sensor.AddMeasurement(time, value.Value);
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

        public async Task SetTargetTemperatureAsync(double temperature)
        {
            var requestObject = new { v = temperature };
            var content = new StringContent(JsonConvert.SerializeObject(requestObject), Encoding.UTF8, "application/json");
            using var response = await HttpClient!.PutAsync(URL_THERMOSTAT_LIVINGROOM_SET, content);
        }
    }
}
