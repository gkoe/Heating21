using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heating.Wpf
{
    public record SensorGetByHttpDto(string Sensor, DateTime Time, string ValueString);
    public class Sensor
    {
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public DateTime Time { get; set; }
    }
}
