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

namespace Services
{
    public class HttpCommunicationService : IHttpCommunicationService
    {
        private const string URL_LIVINGROOM = "http://192.168.0.52/sensor?temperature";
        private readonly IHttpClientFactory _httpClientFactory;
        private HttpClient _httpClient;
        //private readonly JsonSerializerOptions _options;

        public HttpCommunicationService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public event EventHandler<string> MeasurementReceived;
        private bool StopGetByHttp { get; set; }

        public void StartCommunication()
        {
            Task.Run(() => GetMeasurementsByHttpGetUInBackgroundAsync());
        }

        private async Task GetMeasurementsByHttpGetUInBackgroundAsync()
        {
            // temperature_01/state/{"timestamp":1625917023,"value":25.17}
            //{ "sensor": temperature,"time": 2021 - 07 - 17 20:52:50,"value": 24.42 Grad}
            _httpClient = _httpClientFactory.CreateClient();
            Log.Information("HttpCommunicationService started");
            while (!StopGetByHttp)
            {
                try
                {
                    using var response = await _httpClient.GetAsync(URL_LIVINGROOM, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                    var text = await response.Content.ReadAsStringAsync();
                    MeasurementReceived?.Invoke(this, text);
                }
                catch (Exception ex)
                {
                    Log.Error("GetHttpMeasurement; Exception: {Exception}", ex.Message);
                }
                await Task.Delay(10000);
            }
        }

        //private async Task<string> GetMeasurementAsync()
        //{
        //    using var response = await _httpClient.GetAsync(URL_LIVINGROOM, HttpCompletionOption.ResponseHeadersRead);
        //    response.EnsureSuccessStatusCode();
        //    var text = await response.Content.ReadAsStringAsync();
        //    var x = JsonSerializer.Deserialize< HttpMeasurementDto>(text);
        //    //var measurement = await JsonSerializer.DeserializeAsync<HttpMeasurementDto>(text, _options);
        //    return text;
        //}

        public void StopCommunication()
        {
            StopGetByHttp = true;
        }

    }
}
