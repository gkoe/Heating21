
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
        public Sensor[] Sensors { get; init; } = Array.Empty<Sensor>();
        public Actor[] Actors { get; init; } = Array.Empty<Actor>();

        protected ISerialCommunicationService SerialCommunicationService { get; private set; }
        protected IEspHttpCommunicationService EspHttpCommunicationService { get; private set; }
        protected IHomematicHttpCommunicationService HomematicHttpCommunicationService { get; private set; }
        IHubContext<MeasurementsHub> MeasurementsHubContext { get; }
        //public IUnitOfWork UnitOfWork { get; private set; }

        public Sensor GetSensor(string sensorName) => Sensors.FirstOrDefault(s => s.Name == sensorName);
        public Actor GetActor(string actorName) => Actors.FirstOrDefault(s => s.Name == actorName);

        public Sensor GetSensor(SensorName sensorName) => sensorName >= 0 && (int)sensorName < Sensors.Length ? Sensors[(int)sensorName] : null;
        public Actor GetActor(ActorName actorName) => actorName >= 0 && (int)actorName < Actors.Length ? Actors[(int)actorName] : null;


        public event EventHandler<MeasurementDto> NewMeasurement;

        public StateService(IEnumerable<Sensor> sensors, IEnumerable<Actor> actors, IHubContext<MeasurementsHub> measurementsHubContext)
        {
            Sensors = sensors.ToArray();
            Actors = actors.ToArray();
            MeasurementsHubContext = measurementsHubContext;
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


        public async Task SendItemsBySignalRAsync()
        {
            foreach (var sensor in Sensors)
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
            foreach (var actor in Actors)
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
            foreach (var sensor in Sensors)
            {
                if (sensor.LastPersistenceTime.AddSeconds(sensor.PersistenceInterval) <= DateTime.Now)
                {
                    var averageMeasurementValue = sensor.GetAverageMeasurementValuesForPersistenceInterval();
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
            foreach (var actor in Actors)
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
            return Sensors;
        }

        public Actor[] GetActors()
        {
            return Actors;
        }
    }

}
