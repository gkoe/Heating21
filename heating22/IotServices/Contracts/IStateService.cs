using Core.DataTransferObjects;
using Core.Entities;

using IotServices.DataTransferObjects;

using Microsoft.AspNetCore.SignalR;

using Services.Hubs;

namespace Services.Contracts
{
    public interface IStateService
    {
        //public event EventHandler<Measurement> NewMeasurement;

        Sensor[] Sensors { get; }
        Actor[] Actors { get; }

        Sensor GetSensor(ItemEnum sensorName);
        Actor GetActor(ItemEnum actorName);

        //void Init(ISerialCommunicationService serialCommunicationService, IEspHttpCommunicationService espHttpCommunicationService,
        //    IHomematicHttpCommunicationService homematicHttpCommunicationService);

        void Init(IHubContext<SignalRHub> measurementsHubContext);
        Task<bool> SetActorAsync(string actorName, double value);
        MeasurementTimeValue? GetLastMeasurement(ItemEnum itemEnum);

        Task SendItemsBySignalRAsync();

        public Task SendFsmStateChangedAsync(FsmTransition fsmStateChangedInfoDto);
        Measurement[] GetSensorMeasurementsToSave();
        Measurement[] GetActorMeasurementsToSave();

        MeasurementDto[] GetSensorAndActorValues();
        //Measurement[] GetMeasurementsToSave(Item[] items);
        //Measurement[] GetActorMeasurementsToSave();
        //MeasurementDto[] GetSensorAndActorValues();
    }
}