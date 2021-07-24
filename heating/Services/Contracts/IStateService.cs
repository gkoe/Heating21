using Core.DataTransferObjects;

using Services.DataTransferObjects;

using System;

namespace Services.Contracts
{
    public interface IStateService
    {
        SensorWithHistory GetSensor(string sensorName);
        void Init(ISerialCommunicationService serialCommunicationService, IHttpCommunicationService httpCommunicationService);

        public event EventHandler<MeasurementDto> NewMeasurement;
    }
}