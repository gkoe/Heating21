using System;

namespace Heating.Core.DataTransferObjects
{
    public record SensorWithMeasurementDto(string SensorName, DateTime Time, double Value);
}
