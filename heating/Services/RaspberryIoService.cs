using Serilog;

using Services.Contracts;

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class RaspberryIoService : IRaspberryIoService
    {
        const int _resetPin = 16;

        public GpioController GpioController { get; }

        public RaspberryIoService()
        {
            try
            {
                GpioController = new GpioController(PinNumberingScheme.Board);
            }
            catch (Exception)
            {
                Log.Error("RaspberryIoService Constructor;NO RASPI ==> no GpioController");
                GpioController = null;
            }
        }

        public async Task ResetEspAsync()
        {
            if (GpioController == null)
            {
                Log.Error("RaspberryIoService ResetEspAsync; NO RASPI ==> no GpioController");
                return;
            }
            if (GpioController.IsPinOpen(_resetPin))
            {
                GpioController.ClosePin(_resetPin);
            }
            GpioController.OpenPin(_resetPin, PinMode.Output);
            GpioController.Write(_resetPin, PinValue.Low);
            await Task.Delay(1000);
            GpioController.Write(_resetPin, PinValue.High);
            GpioController.ClosePin(_resetPin);
            GpioController.OpenPin(_resetPin, PinMode.Input);
        }
    }
}
