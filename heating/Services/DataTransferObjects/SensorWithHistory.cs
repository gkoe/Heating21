
using Serilog;

using System;

namespace Services.DataTransferObjects
{
    public class SensorWithHistory
    {
        int _actIndex = 0;
        DateTime lastTimeStamp = DateTime.MinValue;

        public string SensorName { get; set; }
        public MeasurementValue[] Measurements = new MeasurementValue[10];
        public double Trend { get; set; }  // umgerechnet in Delta/Stunde

        public DateTime Time { get; private set; }
        public double Value { get; private set; }

        public void AddMeasurement(DateTime time, double value)
        {
            if (Measurements[_actIndex] == null && _actIndex == 0)   // erster Messwert ==> Trend auf 0 setzen
            {
                Trend = 0;
                lastTimeStamp = time;
            }
            else
            {
                double timeFactorToHour = 3600.0 / (time - lastTimeStamp).TotalSeconds;
                int ind = (_actIndex -1 +10) % 10;
                double delta = value - Measurements[ind].Value;
                Trend = (Trend * 0.5 + delta / value * timeFactorToHour)/1.5;
            }
            Measurements[_actIndex] = new MeasurementValue(time, value);
            Log.Information($"Add SensorWithHistory; Sensor: {SensorName}, Value: {value}, Index: {_actIndex}, Trend: {Trend}");
            _actIndex = (_actIndex + 1) % 10;
            // letzte Werte als aktuelle Werte speichern
            Time = time;
            Value = value;
        }
    }



}
