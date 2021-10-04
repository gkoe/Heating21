using Base.Entities;
using System.Collections.Generic;

namespace Core.Entities
{
    public class Sensor : EntityObject
    {
        //public string ThingName { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }

        public List<Measurement> Measurements { get; set; }

        public Sensor()
        {
            Measurements = new List<Measurement>();
        }

        public override string ToString() => $"Sensor: {Name}";

    }
}
