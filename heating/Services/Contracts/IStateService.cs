
using Core.DataTransferObjects;
using Core.Entities;


using System;
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface IStateService
    {
        public event EventHandler<MeasurementDto> NewMeasurement;

        Sensor[] GetSensors();
        Actor[] GetActors();

        Sensor GetSensor(string sensorName);
        Actor GetActor(string actorName);

        void Init(ISerialCommunicationService serialCommunicationService, IEspHttpCommunicationService espHttpCommunicationService, 
            IHomematicHttpCommunicationService homematicHttpCommunicationService);

        Task SendItems();

        public Task SendFsmStateChangedAsync(FsmTransition fsmStateChangedInfoDto);
        Measurement[] GetSensorMeasurementsToSave();
        Measurement[] GetActorMeasurementsToSave();
    }
}