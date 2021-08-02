using Blazored.LocalStorage;

using Common.DataTransferObjects;
using Common.Helper;

using Core.DataTransferObjects;
using Core.Entities;

using Microsoft.AspNetCore.Components.Authorization;

using Newtonsoft.Json;

using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Wasm.Services.Contracts;

namespace Wasm.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _client;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public ApiService(HttpClient client, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
        {
            _client = client;
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
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


    }

}
