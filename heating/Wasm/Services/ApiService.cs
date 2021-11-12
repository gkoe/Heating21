using Core.DataTransferObjects;
using Core.Entities;

using Newtonsoft.Json;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Wasm.Services.Contracts;

namespace Wasm.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _client;
        //private readonly ILocalStorageService _localStorage;
        //private readonly AuthenticationStateProvider _authStateProvider;

        public ApiService(HttpClient client/*, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider */)
        {
            _client = client;
            //_authStateProvider = authStateProvider;
            //_localStorage = localStorage;
        }

        public async Task<bool> ChangeSwitchAsync(string name, bool on)
        {
            var onOffValue = 0;
            if (on)
            {
                onOffValue = 1;
            }
            var request = $"api/actors/change/{name},{onOffValue}";
            var response = await _client.GetAsync(request);
            var contentTemp = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<bool>(contentTemp);
            System.Console.WriteLine($"ApiService;ChangeSwitchAsync; Request: {request}; result: {result}");
            return result;
        }

        public async Task<bool> SetManualOperationAsync(bool on)
        {
            var onOffValue = 0;
            if (on)
            {
                onOffValue = 1;
            }
            //setmanualoperation /
            var request = $"api/ruleengine/setmanualoperation/{onOffValue}";
            var response = await _client.GetAsync(request);
            var contentTemp = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<bool>(contentTemp);
            System.Console.WriteLine($"ApiService;SetManualOperation; Request: {request}; result: {result}");
            return result;
        }

        public async Task<string[]> GetFsmStatesAsync()
        {
            var request = $"api/ruleengine/getfsmstates/";
            var response = await _client.GetAsync(request);
            var contentTemp = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<string[]>(contentTemp);
            System.Console.WriteLine($"ApiService;GetFsmStates; result: {string.Join(';', result)}");
            return result;
        }

        public async Task<MeasurementDto[]> GetMeasurementsAsync(string sensorName, DateTime date)
        {
            var request = $"api/measurements/getbysensoranddate/{sensorName},{date:yyyy-MM-dd}";
            var response = await _client.GetAsync(request);
            var contentTemp = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<MeasurementDto[]>(contentTemp);
            System.Console.WriteLine($"ApiService;GetMeasurementsAsync; {result.Length} measurements read");
            return result;
        }

        public async Task<FsmTransition[]> GetFsmTransitionsAsync(DateTime date)
        {
            var request = $"api/fsmtransitions/getbydate/{date:yyyy-MM-dd}";
            var response = await _client.GetAsync(request);
            var contentTemp = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<FsmTransition[]>(contentTemp);
            System.Console.WriteLine($"ApiService;GetFsmMessagesAsync; {result.Length} fsmtransitions read");
            return result;
        }

        public async Task ResetEspAsync()
        {
            var request = $"api/maintenance/resetesp";
            var response = await _client.GetAsync(request);
            System.Console.WriteLine($"ApiService;ResetEsp; response: {response.StatusCode}");
        }


    }

}
