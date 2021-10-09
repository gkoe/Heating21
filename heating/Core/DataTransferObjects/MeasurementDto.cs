using Base.ExtensionMethods;

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
        public int SensorId { get; set; }
        public string SensorName { get; set; }
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


        public override string ToString()
        {
            return $"{SensorName} {Time.ToShortTimeString()}: {Value}";
        }
    }
}
