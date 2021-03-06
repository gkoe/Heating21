using Base.ExtensionMethods;

using Core.Entities;

using System;

namespace Core.DataTransferObjects
{
    public class MeasurementDto
    {
        private double _value;
        private double _trend;

        public double Value 
        { 
            get
            {
                return _value;
            }
            set
            {
                _value = value.ToLegalDouble();
            }
        }

        public DateTime Time { get; set; }
        public int  ItemId { get; set; }
        public ItemEnum ItemEnum { get; set; }
        public string ItemName { get; set; }
        public double Trend 
        {
            get
            {
                return _trend;
            }
            set
            {
                _trend = value.ToLegalDouble();
            }
        }

        public MeasurementDto()
        {

        }
        public MeasurementDto(Measurement measurement)
        {
            Value = measurement.Value;
            Time = measurement.Time;
            ItemName = measurement.Item?.Name;
            ItemEnum = measurement.ItemEnum;
            if (measurement.Item is Sensor)
            {
                Trend = (measurement.Item as Sensor).Trend;
            }
        }

        public MeasurementDto(string name, DateTime time, double value)
        {
            Time = time;
            Value = value;
        }

        public override string ToString()
        {
            return $"{ItemName} {Time.ToShortTimeString()}: {Value}";
        }
    }
}
