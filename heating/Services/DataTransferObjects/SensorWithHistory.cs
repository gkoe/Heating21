
using System;

namespace Services.DataTransferObjects
{
    public class SensorWithHistory
    {
        int _actIndex = 0;
        public string SensorName { get; set; }
        public MeasurementValue[] Measurements = new MeasurementValue[10];
        public double Trend { get; set; }

        public DateTime Time { get; private set; }
        public double Value { get; private set; }

        public void AddMeasurement(DateTime time, double value)
        {
            if (Measurements[_actIndex] == null && _actIndex == 0)   // erster Messwert ==> Trend auf 0 setzen
            {
                Trend = 0;
            }
            else
            {
                double delta = value - Measurements[(_actIndex - 1) % 10].Value;
                Trend = Trend * 0.8 + delta / value;
            }
            Measurements[_actIndex] = new MeasurementValue(time, value);
            _actIndex = (_actIndex + 1) % 10;
            // letzte Werte als aktuelle Werte speichern
            Time = time;
            Value = value;
        }
    }



}
