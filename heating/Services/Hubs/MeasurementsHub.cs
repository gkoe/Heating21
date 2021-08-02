
using Core.DataTransferObjects;

using Microsoft.AspNetCore.SignalR;

using Serilog;

using Services.Contracts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Hubs
{
    // Send messages from outside a hub
    // https://docs.microsoft.com/en-us/aspnet/core/signalr/hubcontext?view=aspnetcore-2.1
    public class MeasurementsHub : Hub
    {
        public IStateService StateService { get; }

        public MeasurementsHub(IStateService stateService)
        {
            StateService = stateService;
        }

        public async override Task OnConnectedAsync()
        {
            // https://consultwithgriff.com/signalr-connection-ids/
            Log.Information($"client connected, connectionid: {Context.ConnectionId}");
            await StateService.SendSensorsAndActors();
            //await Clients.All.SendAsync("ReceiveMessage", "Hello by SignalR");
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (exception != null)
            {
                Log.Error($"client disconnected, connectionid: {Context.ConnectionId}, exception: {exception.Message}");
            }
            else
            {
                Log.Information($"client disconnected, connectionid: {Context.ConnectionId}");
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendMeasurement(MeasurementDto measurement)
        {
            await Clients.All.SendAsync("ReceiveMeasurement", measurement);
        }

    }
}
