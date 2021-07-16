using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using Radzen;
using Radzen.Blazor;
using System.Collections.Generic;
using Microsoft.JSInterop;
using Blazored.LocalStorage;
using Common.DataTransferObjects;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Authorization;
using Common.Helper;
using System.Linq;
using Microsoft.AspNetCore.SignalR.Client;

namespace Wasm.Pages
{
    public partial class Demo
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private HubConnection hubConnection;
        public List<string> Messages { get; set; }


        protected async override Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            hubConnection = new HubConnectionBuilder()
                       .WithUrl(NavigationManager.ToAbsoluteUri("/measurementshub"))
                       .Build();

            hubConnection.On<string>("ReceiveMessage", (message) =>
            {
                Console.WriteLine($"Message received: {message}");
                var encodedMsg = $"{message}";
                Messages.Add(encodedMsg);
                StateHasChanged();
            });

            await hubConnection.StartAsync();
        }

        async Task Send() =>
        await hubConnection.SendAsync("SendMessage", "XXX");

        public bool IsConnected =>
            hubConnection.State == HubConnectionState.Connected;

        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }
    }
}
