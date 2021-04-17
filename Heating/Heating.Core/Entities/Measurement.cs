using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heating.Core.Entities
{
    public class Measurement : EntityObject
    {
        public double Value { get; set; }

        public DateTime Time { get; set; }

        [ForeignKey(nameof(SensorId))]
        public Sensor Sensor { get; set; }
        public int SensorId { get; set; }

        public override string ToString()
        {
            return $"{Time.ToShortTimeString()}: {Value}";
        }


    }
}
