using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using Services.Contracts;
using Services.Hubs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    public class RuleEngine : BackgroundService
    {
        IServiceProvider ServiceProvider { get; }
        protected ISerialCommunicationService SerialCommunicationService { get; private set; }
        protected IHttpCommunicationService HttpCommunicationService { get; private set; }
        protected IStateService StateService { get; private set; }

        public RuleEngine(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("Ruleengine started");
            //SerialCommunicationService = serialCommunicationService;
            using (var scope = ServiceProvider.CreateScope())
            {
                SerialCommunicationService = scope.ServiceProvider.GetService<ISerialCommunicationService>();
                HttpCommunicationService = scope.ServiceProvider.GetService<IHttpCommunicationService>();
                StateService = scope.ServiceProvider.GetService<IStateService>();
                StateService.Init(SerialCommunicationService, HttpCommunicationService);
            }
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(100, stoppingToken);
                // Auf Änderungen des States reagieren, Timeouts bearbeiten, ...
            }
            //return Task.CompletedTask;
        }

        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            SerialCommunicationService.StopCommunication();
            await base.StopAsync(cancellationToken);
        }


    }
}
