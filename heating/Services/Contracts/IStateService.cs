using Core.DataTransferObjects;

using Services.DataTransferObjects;

using System;
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface IStateService
    {
        SensorWithHistory GetSensor(ItemEnum sensorName);
        public Actor GetActor(ItemEnum actorName);

        void Init(ISerialCommunicationService serialCommunicationService, IHttpCommunicationService httpCommunicationService);

        Task SendSensorsAndActors();

        public event EventHandler<MeasurementDto> NewMeasurement;
        public Task SetActorBySerialCommunication(Actor actor, double value);

        public Task SendFsmStateChangedAsync(FsmStateChangedInfoDto fsmStateChangedInfoDto);
    }
}