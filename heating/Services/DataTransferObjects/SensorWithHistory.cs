
using Serilog;

using System;

namespace Services.DataTransferObjects
{


    public class SensorWithHistory : Item
    {
        public SensorWithHistory(string sensorName, int id) : base(sensorName, id)
        {
        }
    }



}
