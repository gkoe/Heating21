using Serilog;

using System;

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
        HmoLivingroomFirstFloor,
        HmoTemperatureOut,

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
        //const ItemEnum StartActors = ItemEnum.OilBurnerSwitch;
        int _actIndex = 0;
        DateTime lastTimeStamp = DateTime.MinValue;

        public int Id { get; set; }
        public string ItemName { get; private set; }
        public MeasurementValue[] Measurements = new MeasurementValue[100];
        public double Trend { get; set; }  // umgerechnet in Delta/Stunde

        public DateTime Time { get; private set; }
        public double Value { get; private set; }

        public Item(string itemName, int id)
        {
            ItemName = itemName;
            Id = id;
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
            Log.Information($"Add SensorWithHistory; Sensor: {ItemName}, Value: {value}, Index: {_actIndex}, Trend: {Trend}");
            _actIndex = (_actIndex + 1) % Measurements.Length;
            // letzte Werte als aktuelle Werte speichern
            Time = time;
            Value = value;
        }

    }
}
