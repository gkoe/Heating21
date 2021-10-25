using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public enum ItemEnum
    {
        // Sensors
        OilBurnerTemperature,
        BoilerTop,
        BoilerBottom,
        BufferTop,
        BufferBottom,
        TemperatureBefore,
        TemperatureAfter,
        SolarCollector,

        TemperatureFirstFloor,
        TemperatureGroundFloor,
        LivingroomFirstFloor,
        LivingroomGroundFloor,
        HmoLivingroomFirstFloor,
        HmoTemperatureOut,

        // Actors
        OilBurnerSwitch,
        PumpBoiler,
        PumpSolar,
        PumpFirstFloor,
        PumpGroundFloor,
        ValveBoilerBuffer,
        MixerFirstFloorMinus,
        MixerFirstFloorPlus,
        MixerGroundFloorMinus,
        MixerGroundFloorPlus

    }


}
