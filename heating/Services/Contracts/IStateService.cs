using Core.DataTransferObjects;

using Services.DataTransferObjects;

using System;
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface IStateService
    {
        SensorWithHistory GetSensor(string sensorName);
        void Init(ISerialCommunicationService serialCommunicationService, IHttpCommunicationService httpCommunicationService);

        Task SendSensorsAndActors();

        public event EventHandler<MeasurementDto> NewMeasurement;
    }
}