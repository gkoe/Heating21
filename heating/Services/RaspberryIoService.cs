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

        public GpioController GpioController = new GpioController(PinNumberingScheme.Board);

        //public RaspberryIoService()
        //{
        //    if (GpioController.IsPinOpen(_resetPin))
        //    {
        //        GpioController.ClosePin(_resetPin);
        //    }
        //}

        public async Task ResetEspAsync()
        {
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
