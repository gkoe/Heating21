using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DataTransferObjects
{
    public enum ItemEnum
    {
        // Sensors
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

        // Actors
        OilBurnerSwitch,
        PumpBoiler,
        PumpSolar,
        PumpFirstFloor,
        PumpGroundFloor,
        ValveBoilerBuffer,
        MixerFirstFloorMinus,
        MixerFirstFloorPlus,
        MixerGroundFloorMinus,
        MixerGroundFloorPlus

    }

      public class Item
    {
        int _actIndex = 0;
        DateTime lastTimeStamp = DateTime.MinValue;

        public ItemEnum ItemEnum { get; private set; }
        public MeasurementValue[] Measurements = new MeasurementValue[10];
        public double Trend { get; set; }  // umgerechnet in Delta/Stunde

        public DateTime Time { get; private set; }
        public double Value { get; private set; }

        public Item(ItemEnum itemName)
        {
            ItemEnum = itemName;
        }

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
                int ind = (_actIndex - 1 + 10) % 10;
                double delta = value - Measurements[ind].Value;
                if (value == 0)
                {
                    Trend = 1000.0;
                }
                else
                {
                    Trend = (Trend * 0.5 + delta / value * timeFactorToHour) / 1.5;
                }
            }
            Measurements[_actIndex] = new MeasurementValue(time, value);
            Log.Information($"Add SensorWithHistory; Sensor: {ItemEnum}, Value: {value}, Index: {_actIndex}, Trend: {Trend}");
            _actIndex = (_actIndex + 1) % 10;
            // letzte Werte als aktuelle Werte speichern
            Time = time;
            Value = value;
        }

    }
}
