using Base.Entities;

using Serilog;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Core.Entities
{
    public record MeasurementValue(DateTime Time, double Value);

    public enum ItemEnum
    {
        OilBurnerTemperature,
        BoilerTop,
        BoilerBottom,
        SolarCollector,
        LivingroomFirstFloor,
        HmoLivingroomFirstFloor,
        HmoLivingroomFirstFloorSet,
        HmoTemperatureOut,
        BufferTop,
        BufferBottom,
        //TemperatureBefore,
        //TemperatureAfter,
        //TemperatureFirstFloor,
        //TemperatureGroundFloor,
        //LivingroomGroundFloor,

        EndOfSensor = 100,

        OilBurnerSwitch,
        PumpBoiler,
        PumpSolar,
        PumpFirstFloor,
        PumpGroundFloor,
        ValveBoilerBuffer,
        //MixerFirstFloorMinus,
        //MixerFirstFloorPlus,
        //MixerGroundFloorMinus,
        //MixerGroundFloorPlus

    }

    /// <summary>
    /// Item ist die Basisiklasse für Sensor und Actor 
    /// </summary>
    public abstract class Item : EntityObject
    {
        const int MEASUREMENTS_BUFFER_SIZE = 100;
        public double Trend { get; set; }  // umgerechnet in Delta/Stunde
        public int PersistenceInterval { get; set; } = 900;
        public DateTime LastPersistenceTime { get; set; }

        public string Name { get; set; }
        public string Unit { get; set; }

        [NotMapped]
        public ItemEnum ItemEnum { get; private set; }

        public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();

        public Item()
        {
        }

        public Item(ItemEnum itemEnum, string unit = "", int persistenceInterval = 900)
        {
            PersistenceInterval = persistenceInterval;
            ItemEnum = itemEnum;
            Name = itemEnum.ToString();
            Unit = unit;
        }

        [NotMapped]
        public DateTime Time { get; set; }
        [NotMapped]
        public double Value { get; set; }

        [NotMapped]
        public int EnumNumber { get; private set; }

        [NotMapped]
        public double LastValue { get; set; }
        [NotMapped]
        public bool LastValuePersisted { get; set; }

        public Measurement GetAverageMeasurementValuesForPersistenceInterval()
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
                    Time = DateTime.Now
                };
                return measurement;
            }
            return null;
        }

        [NotMapped]
        public MeasurementValue[] MeasurementsBuffer = new MeasurementValue[MEASUREMENTS_BUFFER_SIZE];
        int _actIndex = 0;
        readonly DateTime _lastPersistenceTime = DateTime.MinValue;


        public Measurement AddMeasurement(DateTime time, double value)
        {
            //if (MeasurementsBuffer[_actIndex] == null && _actIndex == 0)   // erster Messwert ==> Trend auf 0 setzen
            //{
            //    Trend = 0;
            //    _lastPersistenceTime = time;
            //}
            //else
            //{
            //    double timeFactorToHour = 3600.0 / (time - _lastPersistenceTime).TotalSeconds;
            //    int index = (_actIndex - 1 + 10) % 10;
            //    double delta = value - MeasurementsBuffer[index].Value;
            //    if (value == 0)
            //    {
            //        Trend = 1000.0;
            //    }
            //    else
            //    {
            //        Trend = (Trend * 0.5 + delta / value * timeFactorToHour) / 1.5;
            //    }
            //}
            MeasurementsBuffer[_actIndex] = new MeasurementValue(time, value);
            var measurementValueBefore10Minutes = GetMeasurementValueBeforeXMinutes(MeasurementsBuffer, _actIndex, 10);
            if (measurementValueBefore10Minutes != null)
            {
                var timeDifference = (DateTime.Now - measurementValueBefore10Minutes.Time).TotalSeconds; ;
                double valueDifference = value - measurementValueBefore10Minutes.Value;
                Trend = valueDifference / timeDifference * 3600;  // Delta pro Stunde
            }
            else
            {
                Trend = double.MinValue;
            }
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
                Value = Value
            };
            return measurement;

        }

        private static MeasurementValue GetMeasurementValueBeforeXMinutes(MeasurementValue[] measurementsBuffer, int actIndex, int minutes)
        {
            var actTime = measurementsBuffer[actIndex].Time;
            var targetTime = actTime.AddMinutes(minutes * (-1));
            int index = ((actIndex - 1) + measurementsBuffer.Length) % measurementsBuffer.Length;
            while (index != actIndex && measurementsBuffer[index] != null && measurementsBuffer[index].Time > targetTime)
            {
                index = ((index - 1) + measurementsBuffer.Length) % measurementsBuffer.Length;
            }
            if (index == actIndex || measurementsBuffer[index] == null)
            {
                return null;
            }
            return measurementsBuffer[index];

        }

        public override string ToString() => $"Item: {Name}";

    }
}
