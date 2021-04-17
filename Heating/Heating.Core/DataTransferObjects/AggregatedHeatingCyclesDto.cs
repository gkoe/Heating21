using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heating.Core.DataTransferObjects
{
    public record AggregatedHeatingCyclesDto(DateTime Date, int NumberOfCycles, double SumOfHours, double ShortestCycleDuration, double LongestCycleDuration);
}
