using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public enum ActorName
    {
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


    public class Actor : Item
    {
        [NotMapped]
        public double LastValue { get; set; }
        [NotMapped]
        public bool LastValuePersisted { get; set; }

        public Actor()  { }
        public Actor(ActorName actorName, string unit = "")
        : base((int)actorName, unit)
        {
            Name = actorName.ToString();
        }


        public override Measurement AddMeasurement(DateTime time, double value)
        {
            LastValue = Value;
            LastValuePersisted = false;
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

        public override string ToString() => $"Actor: {Name}";

    }
}
