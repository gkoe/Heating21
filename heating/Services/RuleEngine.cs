using Core.Contracts;
using Core.DataTransferObjects;
using Core.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence;
using Serilog;
using Services.Contracts;
using Services.ControlComponents;
using Services.DataTransferObjects;
using Services.Hubs;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    public class RuleEngine : BackgroundService
    {
        public static RuleEngine Instance { get; private set; }

        Timer SaveMeasurementsTimer;
            
        IServiceProvider ServiceProvider { get; }

        //private UnitOfWork UnitOfWork { get; }
        public string ConnectionString { get; set; }

        public IConfiguration Configuration { get; set; }
        public ISerialCommunicationService SerialCommunicationService { get; private set; }
        public IRaspberryIoService RaspberryIoService { get; private set; }
        protected IEspHttpCommunicationService EspHttpCommunicationService { get; private set; }
        protected IHomematicHttpCommunicationService HomematicHttpCommunicationService { get; private set; }

        public IStateService StateService { get; private set; }

        public OilBurner OilBurner { get; private set; }
        public HeatingCircuit HeatingCircuit { get; private set; }
        public HotWater HotWater { get; private set; }

        public RuleEngine(IServiceProvider serviceProvider)
        {
            //SaveMeasurementsTimer = new Timer(OnSaveMeasurements, null, 60 * 1000, 900 * 1000); // nach 60 Sekunden starten dann alle 900 Sekunden
            SaveMeasurementsTimer = new Timer(OnSaveMeasurements, null, 10 * 1000, 10 * 1000); // nach 60 Sekunden starten dann alle 900 Sekunden
            ServiceProvider = serviceProvider;
            Configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var appSettingsSection = Configuration.GetSection("ConnectionStrings");
            var dbFileName = appSettingsSection["DbFileName"];
            ConnectionString = $"Data Source={dbFileName}";
            //var dbContext = new ApplicationDbContext(ConnectionString);
            //dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            //UnitOfWork = new UnitOfWork(dbContext);

            var measurementsHubContext = serviceProvider.GetRequiredService<IHubContext<MeasurementsHub>>();
            try
            {
                var sensorsActors = GetSyncedSensorsWithDb().Result;
                StateService = new StateService(measurementsHubContext, sensorsActors);
            }
            catch (Exception ex)
            {
                Log.Error($"Constructor; DB not ready, ex: {ex.Message}");
            }
        }

        private async void OnSaveMeasurements(object state)
        {
            try
            {
                var measurementsToSave = StateService.GetAverageSensorValuesForLast900Seconds();
                Log.Information($"RuleEngine;OnSaveMeasurements;{measurementsToSave} Measurements to save");
                using ApplicationDbContext dbContext = new(ConnectionString);
                using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
                try
                {
                    await unitOfWork.Measurements.AddRangeAsync(measurementsToSave);
                    var count = await unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Log.Error($"RuleEngine,OnSaveMeasurements;Failed to save measurement; ex: {ex.Message}");
                }

            }
            catch (Exception ex)
            {
                Log.Error($"RuleEngine,OnSaveMeasurements;failed to get measurements; ex: {ex.Message}");
            }
        }

        async Task<Sensor[]> GetSyncedSensorsWithDb()
        {
            using ApplicationDbContext dbContext = new (ConnectionString);
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            foreach (var item in Enum.GetValues(typeof(ItemEnum)))
            {
                var sensorName = (ItemEnum)item;
                await unitOfWork.Sensors.UpsertAsync(sensorName.ToString());
            }
            await unitOfWork.SaveChangesAsync();
            var sensors = await unitOfWork.Sensors.GetAsync();
            return sensors.OrderBy(s => s.Name).ToArray();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("ExecuteAsync;started");
            //SerialCommunicationService = serialCommunicationService;
            using (var scope = ServiceProvider.CreateScope())
            {
                IHttpClientFactory httpClientFactory = ServiceProvider.GetRequiredService<IHttpClientFactory>();
                SerialCommunicationService = new SerialCommunicationService(Configuration);
                EspHttpCommunicationService = new EspHttpCommunicationService(httpClientFactory, Configuration);
                HomematicHttpCommunicationService = new HomematicHttpCommunicationService(httpClientFactory, Configuration);
                RaspberryIoService = new RaspberryIoService();

                StateService.Init(SerialCommunicationService, EspHttpCommunicationService, HomematicHttpCommunicationService);
                //StateService.NewMeasurement += StateService_NewMeasurement;
                OilBurner = new OilBurner(StateService, SerialCommunicationService);
                OilBurner.Fsm.StateChanged += Fsm_StateChanged;
                HeatingCircuit = new HeatingCircuit(StateService, SerialCommunicationService, OilBurner);
                HeatingCircuit.Fsm.StateChanged += Fsm_StateChanged;
                HotWater = new HotWater(StateService, SerialCommunicationService, OilBurner);
                HotWater.Fsm.StateChanged += Fsm_StateChanged;

                try
                {
                    await RaspberryIoService.ResetEspAsync();  // Bei Neustart der RuleEngine auch ESP neu starten
                }
                catch (Exception)
                {
                    Log.Error("ExecuteAsync;Raspberry IO not available!");
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

        //private async void StateService_NewMeasurement(object sender, MeasurementDto measurementDto)
        //{
        //    //using ApplicationDbContext dbContext = new (ConnectionString);
        //    //using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
        //    //try
        //    //{
        //    //    await unitOfWork.Measurements.AddAsync(measurementDto);
        //    //    var count = await unitOfWork.SaveChangesAsync();
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    Log.Error($"StateService_NewMeasurement;Failed to save measurement; ex: {ex.Message}");
        //    //}
        //}

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
            var fsmTransition = new FsmTransition
            {
                Time = fsmStateChangedInfoDto.Time,
                Fsm = fsmStateChangedInfoDto.Fsm,
                Input = fsmStateChangedInfoDto.Input,
                LastState = fsmStateChangedInfoDto.LastState,
                ActState = fsmStateChangedInfoDto.ActState,
                InputMessage = fsmStateChangedInfoDto.InputMessage
            };
            using ApplicationDbContext dbContext = new (ConnectionString);
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            try
            {
                await unitOfWork.FsmTransitions.AddAsync(fsmTransition);
                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error($"Fsm_StateChanged;Failed to save transition; ex: {ex.Message}");
            }
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
