using System;
using System.Collections.Generic;
using System.Linq;

using Heating.Core.DataTransferObjects;
using Heating.Core.Entities;

namespace Heating.Core.Services
{
    public class OilBurnerStatistics
    {
        public static OilBurnerHeatingCycleDto[][] GetOilBurnerStatistics(Measurement[] measurements)
        {
            OilBurnerHeatingCycleDto[][] dayValues = measurements
                .OrderBy(m => m.Time)
                .GroupBy(m => m.Time.Date)
                .Select(dayGroup => CalculateValuesForDay(dayGroup.ToList()))
                //.Where(dayValues => dayValues.Length > 0)
                .ToArray();
            return dayValues;
        }

        private static OilBurnerHeatingCycleDto[] CalculateValuesForDay(List<Measurement> measurements)
        {
            List<OilBurnerHeatingCycleDto> heatingCycles = new List<OilBurnerHeatingCycleDto>();
            bool inCycle = false;
            DateTime cycleStart=DateTime.Now;
            DateTime cycleEnd;
            for (int i = 2; i < measurements.Count; i++)
            {
                bool isHeating = measurements[i].Value > 50.0 && measurements[i].Value >= measurements[i - 1].Value && measurements[i - 1].Value >= measurements[i - 2].Value;
                bool isCooling = measurements[i].Value <= measurements[i - 1].Value && measurements[i - 1].Value <= measurements[i - 2].Value;
                var m = measurements[i];
                if (measurements[i].Time > DateTime.Parse("15.01.2021 00:00"))
                {
                    int x = 0;
                }
                if (inCycle && isCooling)
                {
                    if (measurements[i].Value > 80.0)  // Brenner läuft bis über 80°, sonst
                    {   // pendeln die Werte nach dem Abschalten der Pumpen
                        cycleEnd = measurements[i].Time;
                        var duration = (cycleEnd - cycleStart).TotalHours;
                        //if (duration < 0.01)
                        //{
                        //    int y = 0;
                        //}
                        heatingCycles.Add(new OilBurnerHeatingCycleDto(cycleStart, duration));
                        inCycle = false;
                    }
                }
                else if (!inCycle && isHeating)
                {
                    cycleStart = measurements[i].Time;
                    inCycle = true;
                }
            }
            return heatingCycles.ToArray();
        }

    }
}
