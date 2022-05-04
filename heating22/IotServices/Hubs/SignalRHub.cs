using Microsoft.AspNetCore.SignalR;

using Serilog;

namespace Services.Hubs
{
    // Send messages from outside a hub
    // https://docs.microsoft.com/en-us/aspnet/core/signalr/hubcontext?view=aspnetcore-2.1
    public class SignalRHub : Hub
    {
        public async override Task OnConnectedAsync()
        {
            // https://consultwithgriff.com/signalr-connection-ids/
            Log.Information($"client connected, connectionid: {Context.ConnectionId}");
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


    }
}
