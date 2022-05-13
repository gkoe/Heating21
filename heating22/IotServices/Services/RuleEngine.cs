
using Serilog;

using Services.ControlComponents;

namespace Services
{
    public class RuleEngine
    {
        private static readonly Lazy<RuleEngine> lazy = new(() => new RuleEngine());
        public static RuleEngine Instance { get { return lazy.Value; } }

        public async Task Init()
        {
            try
            {
                EspHttpCommunicationService.Instance.RestartEsp();
                await Task.Delay(10000);
                await RaspberryIoService.Instance.ResetEspAsync();  // Bei Neustart der RuleEngine auch ESP neu starten
            }
            catch (Exception)
            {
                Log.Error("Init;Raspberry IO not available!");
            }
            Log.Information("Init;Esp restarted, start FSMs");
            StartFiniteStateMachines();
        }

        public void StartFiniteStateMachines()
        {
            OilBurner.Instance.Start();
            HotWater.Instance.Start();
            HeatingCircuit.Instance.Start();
        }

        private void StopFiniteStateMachines()
        {
            OilBurner.Instance.Stop();
            HotWater.Instance.Stop();
            HeatingCircuit.Instance.Stop();
        }

        public async Task SetTargetTemperatureAsync(double temperature)
        {
            HeatingCircuit.Instance.TargetTemperature = temperature;
            await HomematicHttpCommunicationService.Instance.SetTargetTemperatureAsync(temperature);
        }


        /// <summary>
        /// Mauellen Betrieb ein/ausschalten
        /// </summary>
        /// <param name="on"></param>
        public void SetManualOperation(bool on)
        {
            if (on)  // manueller Betrieb
            {
                StopFiniteStateMachines();
            }
            else
            {
                StartFiniteStateMachines();
            }
        }


    }
}
