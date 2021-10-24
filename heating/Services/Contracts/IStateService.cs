
using Core.DataTransferObjects;
using Core.Entities;

using Services.DataTransferObjects;

using System;
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface IStateService
    {
        public event EventHandler<MeasurementDto> NewMeasurement;

        SensorWithHistory GetSensor(ItemEnum sensorName);
        public Actor GetActor(ItemEnum actorName);

        void Init(ISerialCommunicationService serialCommunicationService, IEspHttpCommunicationService espHttpCommunicationService, 
            IHomematicHttpCommunicationService homematicHttpCommunicationService);

        Task SendSensorsAndActors();

        public Task SendFsmStateChangedAsync(FsmTransition fsmStateChangedInfoDto);

        public Measurement[] GetAverageSensorValuesForLast900Seconds();
    }
}