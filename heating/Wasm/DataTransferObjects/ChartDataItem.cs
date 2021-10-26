using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wasm.DataTransferObjects
{
    public class ChartDataItem
    {
        public int QuarterOfAnHourNumber { get; set; }
        public string HourText => QuarterOfAnHourNumber % 4 == 0 ? (QuarterOfAnHourNumber/4).ToString() : "";
        public double? Value { get; set; }

    }

}


// value => (int)value % 4 == 0 ? ((int)value/4).ToString() : ""