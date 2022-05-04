using Core.DataTransferObjects;
using Core.Entities;

using IotServices.Contracts;
using IotServices.DataTransferObjects;
using IotServices.Services.MqttAdapter;

using Microsoft.AspNetCore.SignalR;

using Serilog;

using Services;
using Services.Contracts;
using Services.Hubs;

namespace IotServices.Services
{
    public class StateService : IStateService
    {
        private static readonly Lazy<StateService> lazy = new(() => new StateService());
        public static StateService Instance { get { return lazy.Value; } }
        IHubContext<SignalRHub>? SignalRHubContext { get; set; } // wird vom IotService per Property injiziert


        private StateService()
        {
            var itemEnums = Enum.GetValues<ItemEnum>();
            Sensors = itemEnums
                .Where(ie => ie < ItemEnum.EndOfSensor)
                .Select(ie => new Sensor(ie, ""))
                .ToArray();
            Actors = itemEnums
                .Where(ie => ie > ItemEnum.EndOfSensor)
                .Select(ie => new Actor(ie, ""))
                .ToArray();
            LastMeasurements = new();
            foreach (var sensor in Sensors)
            {
                LastMeasurements[sensor.ItemEnum] = null;
                sensor.Unit = "°C";
            }
            foreach (var actor in Actors)
            {
                LastMeasurements[actor.ItemEnum] = null;
            }
            // Individuelle Anpassungen
            GetSensor(ItemEnum.OilBurnerTemperature).PersistenceInterval = 60;
        }

        public Dictionary<ItemEnum, MeasurementTimeValue?> LastMeasurements { get; }

        public Sensor[] Sensors { get; }
        public Actor[] Actors { get; }

        public Sensor GetSensor(ItemEnum sensorEnum) => Sensors.Single(s => s.ItemEnum == sensorEnum);
        public Sensor? GetSensor(string itemName) => Sensors.SingleOrDefault(s => s.ItemEnum.ToString() == itemName);
        public Actor GetActor(ItemEnum actorEnum) => Actors.Single(a => a.ItemEnum == actorEnum);
        public Actor? GetActor(string itemName) => Actors.SingleOrDefault(s => s.ItemEnum.ToString() == itemName);

        public Item? GetItem(ItemEnum itemEnum)
        {
            var item = GetSensor(itemEnum) as Item ?? GetActor(itemEnum);
            return item;
        }

        public MeasurementTimeValue? GetLastMeasurement(ItemEnum itemEnum) => LastMeasurements[itemEnum];

        public void Init(IHubContext<SignalRHub> signalRHubContext)
        {
            Log.Information("StateService;Init;StateService started");
            // Sensoren und Aktoren mit DB synchronisieren
            foreach (var sensor in Sensors)
            {
                NoTrackingPersistenceService.Instance.UnitOfWork.Sensors.SynchronizeAsync(sensor);
            }
            foreach (var actor in Actors)
            {
                NoTrackingPersistenceService.Instance.UnitOfWork.Actors.SynchronizeAsync(actor);
            }
            // SignalR-Hub initialisieren
            SignalRHubContext = signalRHubContext;
            MqttService.Instance.MessageReceived += Mqtt_MessageReceived;
        }

        private async void Mqtt_MessageReceived(object? sender, MqttMessage mqttMessage)
        {
            MeasurementTimeValue? measurementDto;
            if (mqttMessage.Topic.ToLower().Contains("shell"))
            {
                measurementDto = new();
                // erzeuge measurement aus shelly-daten
            }
            else
            {
                measurementDto = EspSensorBoxMqttAdapter.ConvertMqttToMeasurementDto(
                        mqttMessage.Topic, mqttMessage.Payload);
            }
            if (measurementDto != null)  // Mqtt-Sensoren, die nicht gebraucht werden ignorieren
            {
                LastMeasurements[measurementDto.ItemEnum] = measurementDto;
                ItemEnum itemEnum = measurementDto.ItemEnum;
                Item? item = GetItem(itemEnum);
                if (item != null)
                {
                    var dbMeasurement = new Measurement
                    {
                        ItemId = item.Id,
                        Time = measurementDto.Time,
                        Value = measurementDto.Value,
                    };
                    await NoTrackingPersistenceService.Instance.UnitOfWork.Measurements.AddAsync(dbMeasurement);
                    await NoTrackingPersistenceService.Instance.UnitOfWork.SaveChangesAsync();
                    if (SignalRHubContext != null)
                    {
                        await SignalRHubContext.Clients.All.SendAsync("ReceiveMeasurement", measurementDto);
                    }
                }
                else
                {
                    Log.Error($"Mqtt_MessageReceived, Item: {itemEnum} not found");
                }
            }
        }

        /// <summary>
        /// Setzt den Aktor per SerialCommunication auf den gewünschten Zustand setzen
        /// </summary>
        /// <param name="actorName"></param>
        /// <param name="value"></param>
        /// <returns>false, wenn der Aktor nicht existiert</returns>
        public async Task<bool> SetActorAsync(string actorName, double value)
        {
            if (!Enum.TryParse<ItemEnum>(actorName, out ItemEnum itemEnum))
            {
                return false;
            }
            Actor? actor = GetActor(itemEnum);
            if (actor == null)
            {
                return false;
            }
            await SerialCommunicationService.Instance.SetActorAsync(actor.Name, value);
            //! Mqtt-Message über Adapter erzeugen lassen (Shelly-Adapter)
            //! Actor per Mqtt setzen
            //MqttService.Instance.Publish(topic, payload);
            Log.Information($"SetActorAsync; {actorName}:{value}");
            return true;
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
                if (SignalRHubContext != null)
                {
                    await SignalRHubContext.Clients.All.SendAsync("ReceiveMeasurement", measurement);
                }
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
                if (SignalRHubContext != null)
                {
                    await SignalRHubContext.Clients.All.SendAsync("ReceiveMeasurement", measurement);
                }
            }
        }

        public async Task SendFsmStateChangedAsync(FsmTransition fsmTransition)
        {
            if (SignalRHubContext != null)
            {
                await SignalRHubContext.Clients.All.SendAsync("ReceiveFsmStateChanged", fsmTransition);
            }
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

        public MeasurementDto[] GetSensorAndActorValues()
        {
            var measurements = new List<MeasurementDto>();
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
                measurements.Add(measurement);
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
                measurements.Add(measurement);
            }
            return measurements.ToArray();
        }






    }

}
