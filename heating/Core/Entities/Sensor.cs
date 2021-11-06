using Serilog;

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Core.Entities
{
    public enum SensorName
    {
        OilBurnerTemperature,
        BoilerTop,
        BoilerBottom,
        BufferTop,
        BufferBottom,
        TemperatureBefore,
        TemperatureAfter,
        SolarCollector,
        TemperatureFirstFloor,
        TemperatureGroundFloor,
        LivingroomFirstFloor,
        LivingroomGroundFloor,
        HmoLivingroomFirstFloor,
        HmoTemperatureOut,
    }

    public class Sensor : Item
    {
        const int MEASUREMENTS_BUFFER_SIZE = 100;

        public double Trend { get; set; }  // umgerechnet in Delta/Stunde

        public int PersistenceInterval { get; set; } = 900;
        public DateTime LastPersistenceTime { get; set; }

        public Measurement GetAverageMeasurementsValuesForPersistenceInterval()
        {
            var measurements = MeasurementsBuffer
                .Where(m => m != null && m.Time > DateTime.Now.AddSeconds(PersistenceInterval * (-1)))
                .ToArray();
            if (measurements.Length > 0)
            {
                var measurement = new Measurement
                {
                    //Item = this,
                    ItemId = Id,
                    Value = measurements.Average(m => m.Value),
                    Time=DateTime.Now
                };
                return measurement;
            }
            return null;
        }

        [NotMapped]
        public MeasurementValue[] MeasurementsBuffer = new MeasurementValue[MEASUREMENTS_BUFFER_SIZE];
        int _actIndex = 0;
        DateTime lastTimeStamp = DateTime.MinValue;


        public override Measurement AddMeasurementToBuffer(DateTime time, double value)
        {
            if (MeasurementsBuffer[_actIndex] == null && _actIndex == 0)   // erster Messwert ==> Trend auf 0 setzen
            {
                Trend = 0;
                lastTimeStamp = time;
            }
            else
            {
                double timeFactorToHour = 3600.0 / (time - lastTimeStamp).TotalSeconds;
                int index = (_actIndex - 1 + 10) % 10;
                double delta = value - MeasurementsBuffer[index].Value;
                if (value == 0)
                {
                    Trend = 1000.0;
                }
                else
                {
                    Trend = (Trend * 0.5 + delta / value * timeFactorToHour) / 1.5;
                }
            }
            MeasurementsBuffer[_actIndex] = new MeasurementValue(time, value);
            Log.Information($"AddMeasurementToBuffer; Item: {Name}, Value: {value}, Index: {_actIndex}, Trend: {Trend}");
            _actIndex = (_actIndex + 1) % MeasurementsBuffer.Length;
            // letzte Werte als aktuelle Werte speichern
            Time = time;
            Value = value;
            var measurement = new Measurement
            {
                ItemId = Id,
                Item = this,
                Time = time,
                Trend = Trend,
                Value = Value
            };
            return measurement;

        }

        public override string ToString() => $"Sensor: {Name}";

    }
}
