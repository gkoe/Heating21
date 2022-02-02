
using Core.DataTransferObjects;
using Core.Entities;


using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface IStateService
    {
        public event EventHandler<MeasurementDto> NewMeasurement;

        //List<Sensor> GetInitialSensors();
        //List<Sensor> GetInitialActors();

        Sensor[] GetSensors();
        Actor[] GetActors();

        Sensor GetSensor(string sensorName);
        Actor GetActor(string actorName);
        Sensor GetSensor(SensorName sensorName);
        Actor GetActor(ActorName actorName);

        void Init(ISerialCommunicationService serialCommunicationService, IEspHttpCommunicationService espHttpCommunicationService, 
            IHomematicHttpCommunicationService homematicHttpCommunicationService);

        Task SendItemsBySignalRAsync();

        public Task SendFsmStateChangedAsync(FsmTransition fsmStateChangedInfoDto);
        Measurement[] GetSensorMeasurementsToSave();
        Measurement[] GetActorMeasurementsToSave();

        MeasurementDto[] GetSensorAndActorValues();
    }
}