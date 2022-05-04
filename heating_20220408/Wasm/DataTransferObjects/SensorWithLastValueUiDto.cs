using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wasm.DataTransferObjects
{
    public class SensorWithLastValueUiDto
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public double Trend { get; set; }
        public string TrendIcon => GetTrendIconForTrend(Trend);

        public SensorWithLastValueUiDto(string name)
        {
            Name = name;
        }

        private static string GetTrendIconForTrend(double trend)
        {
            if (trend > 0.02)
            {
                return "oi-arrow-circle-top";
            }
            else if (trend < -0.02)
            {
                return "oi-arrow-circle-bottom";
            }
            return "oi-arrow-circle-right";
        }

    }
}
