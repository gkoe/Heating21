using Base.Entities;

using Serilog;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public record MeasurementValue(DateTime Time, double Value);

    public abstract class Item : EntityObject
    {

        public string Name { get; set; }
        public string Unit { get; set; }

        [NotMapped]
        public DateTime Time { get; set; }
        [NotMapped]
        public double Value { get; set; }

        public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();

        public abstract Measurement AddMeasurementToBuffer(DateTime time, double value);

    }
}
