using Core.Contracts;
using Core.Entities;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Persistence;

using Serilog;

using Services.Contracts;
using Services.ControlComponents;
using Services.Hubs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    public class RuleEngine : BackgroundService
    {
        public static RuleEngine Instance { get; private set; }

        readonly Timer SaveMeasurementsTimer;
            
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
            SaveMeasurementsTimer = new Timer(OnSaveMeasurements, null, 1 * 1000, 1 * 1000); // nach 60 Sekunden starten dann alle 900 Sekunden
            //SaveMeasurementsTimer = new Timer(OnSaveMeasurements, null, 10 * 1000, 10 * 1000); // nach 60 Sekunden starten dann alle 900 Sekunden
            ServiceProvider = serviceProvider;
            Configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var appSettingsSection = Configuration.GetSection("ConnectionStrings");
            var dbFileName = appSettingsSection["DbFileName"];
            ConnectionString = $"Data Source={dbFileName}";
            //var dbContext = new ApplicationDbContext(ConnectionString);
            //dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            //UnitOfWork = new UnitOfWork(dbContext);

            var measurementsHubContext = serviceProvider.GetRequiredService<IHubContext<MeasurementsHub>>();
            var initialSensors = GetInitialSensors();
            var initialActors = GetInitialActors();
            var sensors = SyncSensors(initialSensors).Result;
            var actors = SyncActors(initialActors).Result;
            try
            {
                StateService = new StateService(sensors, actors, measurementsHubContext);
            }
            catch (Exception ex)
            {
                Log.Error($"Constructor; StateService not ready, ex: {ex.Message}");
            }
        }



        #region Initialize sensors and actors with config and db

        public static List<Sensor> GetInitialSensors()
        {

            var sensors = new List<Sensor>
            {
                new Sensor(SensorName.OilBurnerTemperature, "°C", persistenceInterval: 60),
                new Sensor(SensorName.BoilerTop, "°C"),
                new Sensor(SensorName.BoilerBottom, "°C"),
                new Sensor(SensorName.SolarCollector, "°C"),
                new Sensor(SensorName.LivingroomFirstFloor, "°C"),
                new Sensor(SensorName.HmoLivingroomFirstFloor, "°C"),
                new Sensor(SensorName.HmoTemperatureOut, "°C"),
                new Sensor(SensorName.BufferTop, "°C"),
                new Sensor(SensorName.BufferBottom, "°C"),
            };
            return sensors;
        }
        public static List<Actor> GetInitialActors()
        {

            var actors = new List<Actor>
            {
                new Actor(ActorName.OilBurnerSwitch),
                new Actor(ActorName.PumpBoiler),
                new Actor(ActorName.PumpSolar),
                new Actor(ActorName.PumpFirstFloor),
                new Actor(ActorName.PumpGroundFloor),
                new Actor(ActorName.ValveBoilerBuffer),
            };
            return actors;
        }

        private async Task<Sensor[]> SyncSensors(List<Sensor> initialSensors)
        {
            using ApplicationDbContext dbContext = new(ConnectionString);
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            try
            {
                var dbSensors = (await unitOfWork.Sensors.GetAsync()).ToList();
                Log.Information($"RuleEngine;SyncSensors;{dbSensors.Count} sensors read from db");
                var sensorsToDelete = dbSensors
                    .Where(s => !initialSensors.Any(initialSensor => initialSensor.Name == s.Name))
                    .ToArray();
                foreach (var item in sensorsToDelete)
                {
                    unitOfWork.Sensors.Remove(item);
                }
                var sensorsToInsert = initialSensors
                    .Where(s => !dbSensors.Any(dbSensor => dbSensor.Name == s.Name))
                    .ToArray();
                await unitOfWork.Sensors.AddRangeAsync(sensorsToInsert);
                await unitOfWork.SaveChangesAsync();
                var sensors =  await unitOfWork.Sensors.GetAsync();
                var sensorNames = Enum.GetNames(typeof(SensorName));
                if (sensors.Length != sensorNames.Length)
                {
                    Log.Error($"RuleEngine,SyncSensors; dbSensors: {sensors.Length} !=  enumSensors: {sensorNames.Length}");
                    return null;
                }
                var sensorArray = new Sensor[sensorNames.Length];
                for (int i = 0; i < sensorNames.Length; i++)
                {
                    sensorArray[i] = sensors.Single(s => s.Name == ((SensorName)i).ToString());
                }
                return sensorArray;

            }
            catch (Exception ex)
            {
                Log.Error($"RuleEngine,SyncSensors;Failed to read sensors; ex: {ex.Message}");
            }
            return null;
        }

        private async Task<Actor[]> SyncActors(List<Actor> initialActors)
        {
            using ApplicationDbContext dbContext = new(ConnectionString);
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            try
            {
                var dbActors = (await unitOfWork.Actors.GetAsync()).ToList();
                Log.Information($"RuleEngine;SyncActors;{dbActors.Count} actors read from db");
                var actorsToDelete = dbActors
                    .Where(s => !initialActors.Any(initialActor => initialActor.Name == s.Name))
                    .ToArray();
                foreach (var item in actorsToDelete)
                {
                    unitOfWork.Actors.Remove(item);
                }
                var actorsToInsert = initialActors
                    .Where(s => !dbActors.Any(dbActor => dbActor.Name == s.Name))
                    .ToArray();
                await unitOfWork.Actors.AddRangeAsync(actorsToInsert);
                await unitOfWork.SaveChangesAsync();
                var actors = await unitOfWork.Actors.GetAsync();
                var actorNames = Enum.GetNames(typeof(ActorName));
                if (actors.Length != actorNames.Length)
                {
                    Log.Error($"RuleEngine,SyncActors; dbActors: {actors.Length} !=  enumActors: {actorNames.Length}");
                    return null;
                }
                var actorArray = new Actor[actorNames.Length];
                for (int i = 0; i < actorNames.Length; i++)
                {
                    actorArray[i] = actors.Single(s => s.Name == ((ActorName)i).ToString());
                }
                return actorArray;
            }
            catch (Exception ex)
            {
                Log.Error($"RuleEngine,SyncActors;Failed to read actors; ex: {ex.Message}");
            }
            return null;
        }
        #endregion

        private async void OnSaveMeasurements(object state)
        {
            if (StateService == null) return;
            try
            {
                var sensorMeasurementsToSave = StateService.GetSensorMeasurementsToSave();
                await SaveMeasurements(sensorMeasurementsToSave);
                var actorMeasurementsToSave = StateService.GetActorMeasurementsToSave();
                await SaveMeasurements(actorMeasurementsToSave);
            }
            catch (Exception ex)
            {
                Log.Error($"RuleEngine,OnSaveMeasurements;failed to get measurements; ex: {ex.Message}");
            }
        }

        private async Task SaveMeasurements(IEnumerable<Measurement> measurements)
        {
            if (measurements.Any())
            {
                foreach (var m in measurements)  // Item (Sensor oder Actor) muss sonst von Db in Context geladen werden
                {
                    m.Item = null;
                }
                Log.Information($"RuleEngine;OnSaveMeasurements;{measurements.Count()} Measurements to save");
                using ApplicationDbContext dbContext = new(ConnectionString);
                using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
                try
                {
                    await unitOfWork.Measurements.AddRangeAsync(measurements);
                    var count = await unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Log.Error($"RuleEngine,OnSaveMeasurements;Failed to save Measurement; ex: {ex.Message}");
                }
            }
        }

        //private async Task<List<Sensor>> GetSensorsFromDb()
        //{
        //    using ApplicationDbContext dbContext = new(ConnectionString);
        //    using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
        //    try
        //    {
        //        var sensors = await unitOfWork.Sensors.GetAsync();
        //        Log.Information($"RuleEngine;GetSensorsFromDb;{sensors.Count()} sensors read");
        //        return sensors.ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error($"RuleEngine,GetSensorsFromDb;Failed to read sensors; ex: {ex.Message}");
        //    }
        //    return null;
        //}

        //private async Task<List<Actor>> GetActorsFromDb()
        //{
        //    using ApplicationDbContext dbContext = new(ConnectionString);
        //    using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
        //    try
        //    {
        //        var actors = await unitOfWork.Actors.GetAsync();
        //        Log.Information($"RuleEngine;GetSensorsFromDb;{actors.Count()} actors read");
        //        return actors.ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error($"RuleEngine,GetSensorsFromDb;Failed to read actors; ex: {ex.Message}");
        //    }
        //    return null;
        //}

        //async Task<Sensor[]> SyncSensorsWithDb()
        //{
        //    using ApplicationDbContext dbContext = new(ConnectionString);
        //    using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
        //    foreach (var sensor in StateService.GetSensors())
        //    {
        //        await unitOfWork.Sensors.UpsertAsync(sensor);
        //    }
        //    await unitOfWork.SaveChangesAsync();
        //    var sensors = await unitOfWork.Sensors.GetAsync();
        //    return sensors.OrderBy(s => s.Name).ToArray();
        //}

        //async Task<Actor[]> SyncActorsWithDb()
        //{
        //    using ApplicationDbContext dbContext = new(ConnectionString);
        //    using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
        //    foreach (var actor in StateService.GetActors())
        //    {
        //        await unitOfWork.Actors.UpsertAsync(actor);
        //    }
        //    await unitOfWork.SaveChangesAsync();
        //    var actors = await unitOfWork.Actors.GetAsync();
        //    return actors.OrderBy(s => s.Name).ToArray();
        //}

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
