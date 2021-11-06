using Base.ExtensionMethods;
using Base.Helper;

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
        public int ItemId { get; set; }
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
            Trend = measurement.Trend;
            ItemName = measurement.Item?.Name;
            ItemId = measurement.ItemId;
        }


        public override string ToString()
        {
            return $"{ItemName} {Time.ToShortTimeString()}: {Value}";
        }
    }
}
