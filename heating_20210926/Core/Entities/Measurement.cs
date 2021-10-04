using Common.Persistence.Entities;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class Measurement : EntityObject
    {
        public double Value { get; set; }

        public DateTime Time { get; set; }
        public bool Retained { get; set; }

        [ForeignKey(nameof(SensorId))]
        public Sensor Sensor { get; set; }
        public int SensorId { get; set; }

        public override string ToString()
        {
            return $"{Time.ToShortTimeString()}: {Value}";
        }
    }
}
