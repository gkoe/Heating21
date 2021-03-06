using Core.Contracts;
using Core.DataTransferObjects;
using Core.Entities;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Persistence;

using Serilog;

using Services.Contracts;
using Services.ControlComponents;
using Services.DataTransferObjects;
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
        public static RuleEngine Instance { get; private set; }

        IServiceProvider ServiceProvider { get; }

        private UnitOfWork UnitOfWork { get; }

        protected ISerialCommunicationService SerialCommunicationService { get; private set; }
        public IRaspberryIoService RaspberryIoService { get; private set; }
        protected IHttpCommunicationService HttpCommunicationService { get; private set; }

        protected IStateService StateService { get; private set; }

        public OilBurner OilBurner { get; private set; }
        public HeatingCircuit HeatingCircuit { get; private set; }
        public HotWater HotWater { get; private set; }

        public RuleEngine(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            UnitOfWork = new UnitOfWork(new ApplicationDbContext());
            UnitOfWork.ApplicationDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

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
                OilBurner = new OilBurner(StateService, SerialCommunicationService);
                OilBurner.Fsm.StateChanged += Fsm_StateChanged;
                HeatingCircuit = new HeatingCircuit(StateService, SerialCommunicationService, OilBurner);
                HeatingCircuit.Fsm.StateChanged += Fsm_StateChanged;
                HotWater = new HotWater(StateService, SerialCommunicationService, OilBurner);
                HotWater.Fsm.StateChanged += Fsm_StateChanged;
                try
                {
                    RaspberryIoService = scope.ServiceProvider.GetService<IRaspberryIoService>();
                    await RaspberryIoService.ResetEspAsync();  // Bei Neustart der RuleEngine auch ESP neu starten
                }
                catch (Exception)
                {
                    Log.Error("Raspberry IO not available!");
                }
                StartFiniteStateMachines();
            }
            Instance = this;  // ab jetzt Zugriff über Singleton erlauben
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(100, stoppingToken);
                // Auf Änderungen des States reagieren, Timeouts bearbeiten, ...
            }
            //return Task.CompletedTask;
        }

        public void SetManualOperation(bool on)
        {
            if (on)  // manueller Betrieb
            {
                StopFiniteStateMachines();
            }
            else
            {
                StartFiniteStateMachines();
            }
        }

        private async void Fsm_StateChanged(object sender, FsmTransition fsmStateChangedInfoDto)
        {
            await StateService.SendFsmStateChangedAsync(fsmStateChangedInfoDto);
        }

        private void StartFiniteStateMachines()
        {
            OilBurner.Start();
            HotWater.Start();
            HeatingCircuit.Start();
        }

        private void StopFiniteStateMachines()
        {
            OilBurner.Stop();
            HotWater.Stop();
            HeatingCircuit.Stop();
        }

        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            SerialCommunicationService.StopCommunication();
            await base.StopAsync(cancellationToken);
        }


    }
}
