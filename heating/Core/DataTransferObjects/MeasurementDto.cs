using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DataTransferObjects
{
    public class MeasurementDto
    {
        public double Value { get; set; }
        public DateTime Time { get; set; }
        public int SensorId { get; set; }

        public override string ToString()
        {
            return $"{Time.ToShortTimeString()}: {Value}";
        }
    }
}
