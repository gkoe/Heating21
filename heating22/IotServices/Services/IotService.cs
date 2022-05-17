using Base.Helper;

using Core.Contracts;
using Core.Entities;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Persistence;

using Serilog;

using Services;
using Services.ControlComponents;
using Services.Hubs;
using Core.DataTransferObjects;
using Microsoft.EntityFrameworkCore;

namespace IotServices.Services
{
    /// <summary>
    /// Zentrale KLasse, die Hintergrundverarbeitung verwaltet.
    /// Die meisten Serviceklassen werden als Singletons implementiert.
    /// Nach der Initialisierung der Kommunikationsobjekte und der RuleEngine samt FSMs läuft
    /// der Hintergrundthread in Endlosschleife.
    /// </summary>
    public class IotService : BackgroundService
    {
        public IHubContext<SignalRHub> SignalRHubContext { get; set; }
        //IHubContext<SignalRHub> _signalRHubContext;
        //readonly IHttpClientFactory _httpClientFactory;

        Timer SaveMeasurementsTimer { get; }

        //IServiceProvider? ServiceProvider { get; }

        //private UnitOfWork UnitOfWork { get; }
        public string ConnectionString { get; set; }

        public event EventHandler<MeasurementDto>? NewMeasurement;

        //public IConfiguration Configuration { get; set; }
        //public ISerialCommunicationService SerialCommunicationService { get; private set; }
        //public IRaspberryIoService RaspberryIoService { get; private set; }
        //protected IEspHttpCommunicationService EspHttpCommunicationService { get; private set; }
        //protected IHomematicHttpCommunicationService HomematicHttpCommunicationService { get; private set; }

        //public IStateService StateService { get; private set; }

        public OilBurner OilBurner { get; private set; }
        public HeatingCircuit HeatingCircuit { get; private set; }
        public HotWater HotWater { get; private set; }
        public IServiceProvider ServiceProvider { get; }

        public IotService(IServiceProvider serviceProvider) 
        {
            ServiceProvider = serviceProvider;
            SignalRHubContext = ServiceProvider.GetRequiredService<IHubContext<SignalRHub>>();
            ConnectionString = ConfigurationHelper.GetConfiguration("DefaultConnection", "ConnectionStrings");
            OilBurner = OilBurner.Instance;
            OilBurner.Fsm.StateChanged += Fsm_StateChanged;
            HeatingCircuit = HeatingCircuit.Instance;
            HeatingCircuit.Fsm.StateChanged += Fsm_StateChanged;
            HotWater = HotWater.Instance;
            HotWater.Fsm.StateChanged += Fsm_StateChanged;
            SaveMeasurementsTimer = new Timer(OnSaveMeasurements, null, 10 * 1000, 10 * 1000); // nach 60 Sekunden starten dann alle 900 Sekunden
        }

        #region Initialize sensors and actors with config and db


        /// <summary>
        /// DB-Sensoren mit Sensoren aus der Programmlogik synchronisieren.
        /// Werden im Code Sensoren gelöscht oder neu angelegt erfolgt die
        /// Anpassung der Sensoren in der Datenbank.
        /// </summary>
        /// <param name="initialSensors">Sensoren, die sich aus dem ItemEnum ergeben</param>
        /// <returns>Aktuell verwendete Sensoren</returns>
        private async Task<Sensor[]> SyncSensors(Sensor[] initialSensors)
        {
            using ApplicationDbContext dbContext = new(ConnectionString);
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            try
            {
                var dbSensors = (await unitOfWork.Sensors.GetAsync()).ToList();
                Log.Information($"IotService;SyncSensors;{dbSensors.Count} sensors read from db");
                // Zuerst jene Sensoren aus dem DbContext löschen, die nicht mehr im Code vorhanden sind
                var sensorsToDelete = dbSensors
                    .Where(s => !initialSensors.Any(initialSensor => initialSensor.Name == s.Name))
                    .ToArray();
                foreach (var item in sensorsToDelete)
                {
                    unitOfWork.Sensors.Remove(item);
                }
                // Dann neu im Code aufgenommene Sensoren zu den DB-Sensoren hinzufügen
                var sensorsToInsert = initialSensors
                    .Where(s => !dbSensors.Any(dbSensor => dbSensor.Name == s.Name))
                    .ToArray();
                await unitOfWork.Sensors.AddRangeAsync(sensorsToInsert);
                await unitOfWork.SaveChangesAsync();
                var resultSensors = await unitOfWork.Sensors.GetAsync();
                //var itemEnums = Enum.GetValues<ItemEnum>();
                //var sensorNames = itemEnums
                //    .Where(ie => ie < ItemEnum.EndOfSensor)
                //    .Select(ie => new Sensor(ie, ""))
                //    .ToArray();
                //if (resultSensors.Length != sensorNames.Length)
                //{
                //    Log.Error($"IotService,SyncSensors; dbSensors: {resultSensors.Length} !=  enumSensors: {sensorNames.Length}");
                //    return null;
                //}
                //var sensorArray = new Sensor[sensorNames.Length];
                //for (int i = 0; i < sensorNames.Length; i++)
                //{
                //    sensorArray[i] = resultSensors.Single(s => s.Name == sensorNamesi]);
                //}
                return resultSensors;
            }
            catch (Exception ex)
            {
                Log.Error($"IotService,SyncSensors;Failed to read sensors; ex: {ex.Message}");
            }
            return Array.Empty<Sensor>();
        }

        /// <summary>
        /// DB-Aktoren mit Aktoren aus der Programmlogik synchronisieren.
        /// Werden im Code Aktoren gelöscht oder neu angelegt erfolgt die
        /// Anpassung der Aktoren in der Datenbank.
        /// </summary>
        /// <param name="initialActors">Aktoren, die sich aus dem ItemEnum ergeben</param>
        /// <returns>Aktuell verwendete Aktoren</returns>
        private async Task<Actor[]> SyncActors(Actor[] initialActors)
        {
            using ApplicationDbContext dbContext = new(ConnectionString);
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            try
            {
                var dbActors = (await unitOfWork.Actors.GetAsync()).ToList();
                Log.Information($"IotService;SyncActors;{dbActors.Count} actors read from db");
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
                var resultActors = await unitOfWork.Actors.GetAsync();
                //var actorNames = Enum.GetNames(typeof(ActorName));
                //if (resultActors.Length != actorNames.Length)
                //{
                //    Log.Error($"IotService,SyncActors; dbActors: {resultActors.Length} !=  enumActors: {actorNames.Length}");
                //    return null;
                //}
                //var actorArray = new Actor[actorNames.Length];
                //for (int i = 0; i < actorNames.Length; i++)
                //{
                //    actorArray[i] = resultActors.Single(s => s.Name == ((ActorName)i).ToString());
                //}
                return resultActors;
            }
            catch (Exception ex)
            {
                Log.Error($"IotService,SyncActors;Failed to read actors; ex: {ex.Message}");
            }
            return Array.Empty<Actor>();
        }
        #endregion

        #region persist measurements

        /// <summary>
        /// Die Messwerte der Sensoren und Aktoren werden regelmäßig in
        /// die Datenbank gespeichert.
        /// </summary>
        /// <param name="state"></param>
        private async void OnSaveMeasurements(object? state)
        {
            //if (StateService.Instance == null) return;
            try
            {
                var sensorMeasurementsToSave = StateService.Instance.GetSensorMeasurementsToSave();
                await SaveMeasurements("sensors", sensorMeasurementsToSave);
                var actorMeasurementsToSave = StateService.Instance.GetActorMeasurementsToSave();
                await SaveMeasurements("actors",actorMeasurementsToSave);
            }
            catch (Exception ex)
            {
                Log.Error($"IotService,OnSaveMeasurements;failed to get measurements; ex: {ex.Message}");
            }
        }

        /// <summary>
        /// Messwerte in DB speichern
        /// </summary>
        /// <param name="measurements"></param>
        /// <returns></returns>
        private static async Task SaveMeasurements(string source, IEnumerable<Measurement> measurements)
        {
            if (measurements.Any())
            {
                foreach (var m in measurements)  // Item (Sensor oder Actor) muss sonst von Db in Context geladen werden
                {
                    m.Item = null;
                }
                Log.Information($"IotService;OnSaveMeasurements for {source};{measurements.Count()} Measurements to save");
                using IUnitOfWork unitOfWork = new UnitOfWork();
                try
                {
                    await unitOfWork.Measurements.AddRangeAsync(measurements);
                    var count = await unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Log.Error($"IotService,OnSaveMeasurements;Failed to save Measurement; ex: {ex.Message}");
                }
            }
        }

        #endregion


        /// <summary>
        /// Startpunkt der Hintergrundabläufe
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("IotService;ExecuteAsync;started");
            using var scope = ServiceProvider.CreateScope();
            IHttpClientFactory httpClientFactory = ServiceProvider.GetRequiredService<IHttpClientFactory>();
            _ = StateService.Instance;
            SerialCommunicationService.Instance.StartCommunication();
            SerialCommunicationService.Instance.MeasurementReceived += MeasurementReceivedAsync;
            EspHttpCommunicationService.Instance.Init(httpClientFactory);
            EspHttpCommunicationService.Instance.MeasurementReceived += MeasurementReceivedAsync;
            HomematicHttpCommunicationService.Instance.Init(httpClientFactory);
            HomematicHttpCommunicationService.Instance.MeasurementReceived += HomematicHttpCommunicationService_MeasurementReceived;
            HomematicHttpCommunicationService.Instance.MeasurementReceived += MeasurementReceivedAsync;
            HomematicHttpCommunicationService.Instance.StartCommunication();
            _ = RaspberryIoService.Instance;
            StateService.Instance.Init(SignalRHubContext);
            StateService.Instance.Sensors = await SyncSensors(StateService.Instance.Sensors);
            StateService.Instance.Actors = await SyncActors(StateService.Instance.Actors);
            await RuleEngine.Instance.Init();
            //_ = new Timer(OnSaveMeasurements, null, 1 * 1000, 1 * 1000); // nach 60 Sekunden starten dann alle 900 Sekunden
            //SaveMeasurementsTimer = new Timer(OnSaveMeasurements, null, 10 * 1000, 10 * 1000); // nach 60 Sekunden starten dann alle 900 Sekunden
            //Instance = this;  // ab jetzt Zugriff über Singleton erlauben
            int round = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(100, stoppingToken);
                round++;
                if (round >= 100)  // alle 10 Sekunden
                {
                    Log.Information($"IotService;ExecuteAsync;check oilburner and heating");
                    OilBurner.CheckOilBurner();
                    HeatingCircuit.CheckHeating();
                    round = 0;
                }
                // Auf Änderungen des States reagieren, Timeouts bearbeiten, ...
            }
            //return Task.CompletedTask;
        }

        private async void MeasurementReceivedAsync(object? sender, MeasurementDto measurement)
        {
            if (measurement != null)
            {
                NewMeasurement?.Invoke(this, measurement);
                await SignalRHubContext.Clients.All.SendAsync("ReceiveMeasurement", measurement);
                Log.Information($"StateService;MeasurementReceivedAsync; measurement received: {measurement}");
            }
            else
            {
                Log.Error($"StateService;MeasurementReceived; measurement is null");
            }
            await Task.CompletedTask;
        }




        /// <summary>
        /// Wurde der Wert des Heizthermostates geändert, den neuen Wert in die Steuerung übernehmen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="measurementDto"></param>
        private void HomematicHttpCommunicationService_MeasurementReceived(object? sender, MeasurementDto measurementDto)
        {
            if (measurementDto.ItemName == ItemEnum.HmoLivingroomFirstFloorSet.ToString())
            {
                HeatingCircuit.TargetTemperature = measurementDto.Value;
            }
        }


        private async void Fsm_StateChanged(object? sender, FsmTransition fsmStateChangedInfoDto)
        {
            await StateService.Instance.SendFsmStateChangedAsync(fsmStateChangedInfoDto);
            var fsmTransition = new FsmTransition
            {
                Time = fsmStateChangedInfoDto.Time,
                Fsm = fsmStateChangedInfoDto.Fsm,
                Input = fsmStateChangedInfoDto.Input,
                LastState = fsmStateChangedInfoDto.LastState,
                ActState = fsmStateChangedInfoDto.ActState,
                InputMessage = fsmStateChangedInfoDto.InputMessage
            };
            using IUnitOfWork unitOfWork = new UnitOfWork();
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

        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            SerialCommunicationService.Instance.StopCommunication();
            await base.StopAsync(cancellationToken);
        }

    }
}