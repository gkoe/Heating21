
using Core.DataTransferObjects;
using Core.Entities;

using Microsoft.AspNetCore.SignalR;
using Serilog;
using Services.Contracts;
using Services.Hubs;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Services
{

    public class StateService : IStateService
    {
        public ConcurrentDictionary<string, Sensor> Sensors { get; init; } = new ConcurrentDictionary<string, Sensor>();
        public ConcurrentDictionary<string, Actor> Actors { get; init; } = new ConcurrentDictionary<string, Actor>();

        protected ISerialCommunicationService SerialCommunicationService { get; private set; }
        protected IEspHttpCommunicationService EspHttpCommunicationService { get; private set; }
        protected IHomematicHttpCommunicationService HomematicHttpCommunicationService { get; private set; }
        IHubContext<MeasurementsHub> MeasurementsHubContext { get; }
        //public IUnitOfWork UnitOfWork { get; private set; }

        public Sensor GetSensor(string sensorName)
        {
            if (!Sensors.ContainsKey(sensorName))
            {
                return null;
            }
            return Sensors[sensorName];
        }
        public Actor GetActor(string actorName)
        {
            if (!Actors.ContainsKey(actorName))
            {
                return null;
            }
            return Actors[actorName];
        }

        public event EventHandler<MeasurementDto> NewMeasurement;

        public StateService(IHubContext<MeasurementsHub> measurementsHubContext)
        {
            InitSensors();
            InitActors();
            MeasurementsHubContext = measurementsHubContext;
        }

        private void InitSensors()
        {
            Sensors[SensorName.OilBurnerTemperature.ToString()] = new Sensor { Name = SensorName.OilBurnerTemperature.ToString(), PersistenceInterval = 60 };
            Sensors[SensorName.BoilerTop.ToString()] = new Sensor { Name = SensorName.BoilerTop.ToString(), PersistenceInterval = 900 };
            Sensors[SensorName.BoilerBottom.ToString()] = new Sensor { Name = SensorName.BoilerBottom.ToString(), PersistenceInterval = 900 };
            Sensors[SensorName.SolarCollector.ToString()] = new Sensor { Name = SensorName.SolarCollector.ToString(), PersistenceInterval = 900 };
            Sensors[SensorName.LivingroomFirstFloor.ToString()] = new Sensor { Name = SensorName.LivingroomFirstFloor.ToString(), PersistenceInterval = 900 };
            Sensors[SensorName.HmoLivingroomFirstFloor.ToString()] = new Sensor { Name = SensorName.HmoLivingroomFirstFloor.ToString(), PersistenceInterval = 900 };
            Sensors[SensorName.HmoTemperatureOut.ToString()] = new Sensor { Name = SensorName.HmoTemperatureOut.ToString(), PersistenceInterval = 900 };
            Sensors[SensorName.BufferTop.ToString()] = new Sensor { Name = SensorName.BufferTop.ToString(), PersistenceInterval = 900 };
            Sensors[SensorName.BufferBottom.ToString()] = new Sensor { Name = SensorName.BufferBottom.ToString(), PersistenceInterval = 900 };
        }

        private void InitActors()
        {
            Actors[ActorName.OilBurnerSwitch.ToString()] = new Actor { Name = ActorName.OilBurnerSwitch.ToString() };
            Actors[ActorName.PumpBoiler.ToString()] = new Actor { Name = ActorName.PumpBoiler.ToString() };
            Actors[ActorName.PumpSolar.ToString()] = new Actor { Name = ActorName.PumpSolar.ToString() };
            Actors[ActorName.PumpFirstFloor.ToString()] = new Actor { Name = ActorName.PumpFirstFloor.ToString() };
            Actors[ActorName.PumpGroundFloor.ToString()] = new Actor { Name = ActorName.PumpGroundFloor.ToString() };
            Actors[ActorName.ValveBoilerBuffer.ToString()] = new Actor { Name = ActorName.ValveBoilerBuffer.ToString() };
        }

        public void Init(ISerialCommunicationService serialCommunicationService, IEspHttpCommunicationService espHttpCommunicationService,
            IHomematicHttpCommunicationService homematicHttpCommunicationService)
        {
            Log.Information("StateService;Init;StateService started");
            SerialCommunicationService = serialCommunicationService;
            SerialCommunicationService.MeasurementReceived += MeasurementReceivedAsync;
            SerialCommunicationService.StartCommunication();
            EspHttpCommunicationService = espHttpCommunicationService;
            EspHttpCommunicationService.MeasurementReceived += MeasurementReceivedAsync;
            EspHttpCommunicationService.StartCommunication();
            HomematicHttpCommunicationService = homematicHttpCommunicationService;
            HomematicHttpCommunicationService.MeasurementReceived += MeasurementReceivedAsync;
            HomematicHttpCommunicationService.StartCommunication();
        }

        private async void MeasurementReceivedAsync(object sender, MeasurementDto measurement)
        {
            if (measurement != null)
            {
                NewMeasurement?.Invoke(this, measurement);
                await MeasurementsHubContext.Clients.All.SendAsync("ReceiveMeasurement", measurement);
                Log.Information($"StateService;MeasurementReceivedAsync; measurement received: {measurement}");
            }
            else
            {
                Log.Error($"StateService;MeasurementReceived; measurement is null");
            }
            await Task.CompletedTask;
        }


        public async Task SendItems()
        {
            foreach (var sensor in Sensors.Values)
            {
                MeasurementDto measurement = new()
                {
                    ItemId = sensor.Id,
                    ItemName = sensor.Name,
                    Time = sensor.Time,
                    Trend = sensor.Trend,
                    Value = sensor.Value
                };
                await MeasurementsHubContext.Clients.All.SendAsync("ReceiveMeasurement", measurement);
            }
            foreach (var actor in Actors.Values)
            {
                MeasurementDto measurement = new()
                {
                    ItemId = actor.Id,
                    ItemName = actor.Name,
                    Time = actor.Time,
                    Value = actor.Value
                };
                await MeasurementsHubContext.Clients.All.SendAsync("ReceiveMeasurement", measurement);
            }
        }

        public async Task SendFsmStateChangedAsync(FsmTransition fsmTransition)
        {
            await MeasurementsHubContext.Clients.All.SendAsync("ReceiveFsmStateChanged", fsmTransition);
        }

        public Measurement[] GetSensorMeasurementsToSave()
        {
            var measurementsToPersist = new List<Measurement>();
            foreach (var sensor in Sensors.Values)
            {
                if (sensor.LastPersistenceTime.AddSeconds(sensor.PersistenceInterval) <= DateTime.Now)
                {
                    var averageMeasurementValue = sensor.GetAverageMeasurementsValuesForPersistenceInterval();
                    if (averageMeasurementValue != null)
                    {
                        measurementsToPersist.Add(averageMeasurementValue);
                        sensor.LastPersistenceTime = DateTime.Now;
                    }
                }
            }
            return measurementsToPersist.ToArray();
        }

        public Measurement[] GetActorMeasurementsToSave()
        {
            var measurementsToPersist = new List<Measurement>();
            foreach (var actor in Actors.Values)
            {
                if (!actor.LastValuePersisted)
                {
                    var beforeMeasurement = new Measurement
                    {
                        Item = actor,
                        ItemId = actor.Id,
                        Time = DateTime.Now,
                        Value = actor.LastValue
                    };
                    measurementsToPersist.Add(beforeMeasurement);
                    var afterMeasurement = new Measurement
                    {
                        Item = actor,
                        ItemId = actor.Id,
                        Time = DateTime.Now,
                        Value = actor.Value
                    };
                    measurementsToPersist.Add(afterMeasurement);
                    actor.LastValuePersisted = true;
                }
            }
            return measurementsToPersist.ToArray();
        }

        public Sensor[] GetSensors()
        {
            return Sensors.Values.ToArray();
        }

        public Actor[] GetActors()
        {
            return Actors.Values.ToArray();
        }
    }

}
