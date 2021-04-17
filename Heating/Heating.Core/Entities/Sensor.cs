using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heating.Core.Entities
{
    public class Sensor : EntityObject
    {
        public string Name { get; set; }
        public List<Measurement> Measurements { get; set; } = new List<Measurement>();

    }
}
