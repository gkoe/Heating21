using Core.DataTransferObjects;
using Microsoft.AspNetCore.Components;

using System.Threading.Tasks;

using Wasm.Services.Contracts;

namespace Wasm.DataTransferObjects
{
    public class ActorUiDto
    {
        public IApiService ApiService { get; set; }

        public string Name { get; set; }
        public bool IsDisabled => WaitingForResponse || !InManualMode;

        public bool WaitingForResponse { get; set; }
        public bool InManualMode { get; set; }


        private bool _isOn = false;

        public bool IsOn
        {
            get
            {
                return _isOn;
            }
            set
            {
                _isOn = value;
                //WaitingForResponse = true;
                //ApiService.ChangeSwitchAsync(Name, _isOn);
            }
        }


        public ActorUiDto(string name)
        {
            Name = name;
        }

        public void NewActorValueReceived(MeasurementDto measurement)
        {
            WaitingForResponse = false;
            _isOn = measurement.Value == 1;
            System.Console.WriteLine($"{Name}, NewActorValueReceived, IsOn: {IsOn}, WaitingForResponse: {WaitingForResponse}");
        }

        public async Task SwitchActorPerApiAsync()
        {
            await ApiService.ChangeSwitchAsync(Name, IsOn);
            WaitingForResponse = true;
            System.Console.WriteLine($"{Name}, SwitchActorPerApi, IsOn: {IsOn}, WaitingForResponse: {WaitingForResponse}");
        }

    }
}
