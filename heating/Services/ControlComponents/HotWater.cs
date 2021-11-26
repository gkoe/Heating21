
using Core.Entities;

using Serilog;

using Services.Contracts;
using Services.Fsm;

using System;
using System.Linq;

namespace Services.ControlComponents
{
    public sealed class HotWater
    {

        const double BOILER_VERY_HOT = 85.0;
        const double BOILER_HOT = 56.0;
        const double BOILER_COLD = 53.0;

        public enum State { AllOff, HeatBoilerByBurner, HeatBoilerBySolar, HeatBufferBySolar, CoolBurner};
        public enum Input
        {
            IsBoilerToHeatByBurner, IsntBoilerToHeatByBurner, IsBoilerHot, IsBurnerToCool, IsntBurnerToCool, IsBoilerToHeatBySolar,
            IsBoilerVeryHot, IsntBufferToHeatBySolar, IsntBoilerToHeatBySolar
        };
        public IStateService StateService { get; }
        public FiniteStateMachine Fsm { get; set; }
        public OilBurner OilBurner { get; }
        public ISerialCommunicationService SerialCommunicationService { get; }

        public HotWater(IStateService stateService, ISerialCommunicationService serialCommunicationService, OilBurner oilBurner)
        {
            StateService = stateService;
            SerialCommunicationService = serialCommunicationService;
            OilBurner = oilBurner;
            InitFsm();
        }

        public void Start()
        {
            Fsm.Start(State.AllOff);
        }
        public void Stop()
        {
            Fsm.Stop();
        }



        /// <summary>
        /// Fsm wird über Enums angelegt
        /// </summary>
        void InitFsm()
        {

            Enum[] stateEnums = Enum.GetValues<State>().Select(e => (Enum)e).ToArray();
            Enum[] inputEnums = Enum.GetValues<Input>().Select(e => (Enum)e).ToArray();
            Fsm = new FiniteStateMachine(nameof(HotWater), stateEnums, inputEnums);
            try
            {
                // Triggermethoden bei Inputs definieren
                Fsm.GetInput(Input.IsntBoilerToHeatByBurner).TriggerMethod = IsntBoilerToHeatByBurner;
                Fsm.GetInput(Input.IsBoilerToHeatByBurner).TriggerMethod = IsBoilerToHeatByBurner;
                Fsm.GetInput(Input.IsBoilerHot).TriggerMethod = IsBoilerHot;
                Fsm.GetInput(Input.IsBurnerToCool).TriggerMethod = IsBurnerToCool;
                Fsm.GetInput(Input.IsntBurnerToCool).TriggerMethod = IsntBurnerToCool;
                Fsm.GetInput(Input.IsBoilerToHeatBySolar).TriggerMethod = IsBoilerToHeatBySolar;
                Fsm.GetInput(Input.IsntBoilerToHeatBySolar).TriggerMethod = IsntBoilerToHeatBySolar;
                Fsm.GetInput(Input.IsBoilerVeryHot).TriggerMethod = IsBoilerVeryHot;
                Fsm.GetInput(Input.IsntBufferToHeatBySolar).TriggerMethod = IsntBufferToHeatBySolar;
                // Übergänge definieren
                Fsm.AddTransition(State.AllOff, State.HeatBoilerByBurner, Input.IsBoilerToHeatByBurner);
                Fsm.AddTransition(State.HeatBoilerByBurner, State.AllOff, Input.IsntBoilerToHeatByBurner);
                Fsm.AddTransition(State.AllOff, State.HeatBoilerBySolar, Input.IsBoilerToHeatBySolar);
                Fsm.AddTransition(State.AllOff, State.CoolBurner, Input.IsBurnerToCool);
                Fsm.AddTransition(State.HeatBoilerByBurner, State.AllOff, Input.IsBoilerHot);
                Fsm.AddTransition(State.HeatBoilerByBurner, State.HeatBoilerBySolar, Input.IsBoilerToHeatBySolar);
                Fsm.AddTransition(State.HeatBoilerBySolar, State.AllOff, Input.IsntBoilerToHeatBySolar);
                Fsm.AddTransition(State.HeatBoilerBySolar, State.HeatBufferBySolar, Input.IsBoilerVeryHot);
                Fsm.AddTransition(State.HeatBufferBySolar, State.AllOff, Input.IsntBufferToHeatBySolar);
                Fsm.AddTransition(State.CoolBurner, State.AllOff, Input.IsntBurnerToCool);
                // Aktionen festlegen
                Fsm.GetState(State.HeatBoilerByBurner).OnEnter += DoHeatBoilerByBurner;
                Fsm.GetState(State.HeatBoilerBySolar).OnEnter += DoHeatBoilerBySolar;
                Fsm.GetState(State.HeatBufferBySolar).OnEnter += DoHeatBufferBySolar;
                Fsm.GetState(State.CoolBurner).OnEnter += DoHeatBoilerByBurner;
                Fsm.GetState(State.AllOff).OnEnter += DoAllOff;
            }
            catch (Exception ex)
            {
                Log.Error($"HotWater;Fehler bei Init FsmHeatingCircuit, ex: {ex.Message}");
            }
        }
        #region TriggerMethoden

        public (bool, string) IsBoilerToHeatBySolar()
        {
            var solar = StateService.GetSensor(SensorName.SolarCollector).Value;
            var boilerBottom = StateService.GetSensor(SensorName.BoilerBottom).Value;
            if (boilerBottom >= BOILER_VERY_HOT)
            {
                return (false, $"Boiler is very hot (bottom: {boilerBottom})");
            }
            if (solar > (boilerBottom + 10.0))
            {
                return (true,$"Solar ({solar}) is ready to heat boiler (bottom: {boilerBottom})");
            }
            return  (false, "");
        }

        public (bool, string) IsntBoilerToHeatBySolar()
        {
            var solar = StateService.GetSensor(SensorName.SolarCollector).Value;
            var boilerBottom = StateService.GetSensor(SensorName.BoilerBottom).Value;
            //if (boilerBottom >= BOILER_VERY_HOT)
            //{
            //    return (true, $"Boiler is very hot (bottom: {boilerBottom})");
            //}
            if(solar < boilerBottom + 5.0)
            {
                return (true, $"Solar is too cold ({solar}) to heat boiler (bottom: {boilerBottom})");
            }
            return (false, "");
        }

        private (bool, string) IsBurnerToCool() => OilBurner.IsHeatedToTooHot();
        public (bool, string) IsntBurnerToCool() => OilBurner.IsCooledToHot();

        private (bool, string) IsBoilerHot()
        {
            var temperature = StateService.GetSensor(SensorName.BoilerTop).Value;
            if (temperature >= BOILER_HOT)
            {
                return (true, $"Boiler is hot: {temperature})");
            }
            return (false, "");
        }

        private (bool, string) IsntBoilerToHeatByBurner()
        {
            if (OilBurner.IsHeatedToReady().Item1)
            {
                return (false, "OilBurner is ready");
            }
            return (true, "Oilburner is too cold");
        }

        private (bool, string) IsBoilerToHeatByBurner()
        {
            if (!OilBurner.IsHeatedToReady().Item1)
            {
                return (false, "OilBurner isn't ready");
            }
            var temperature = StateService.GetSensor(SensorName.BoilerTop).Value;
            if (temperature <= BOILER_COLD)
            {
                return (true, $"Boiler is to heat by burner: {temperature})");
            }
            return (false, "");
        }

        private (bool, string) IsBoilerVeryHot()
        {
            var temperature = StateService.GetSensor(SensorName.BoilerTop).Value;
            if (temperature >= BOILER_VERY_HOT)
            {
                return (true, $"Boiler is very hot: {temperature})");
            }
            return (false, "");
        }

        private (bool, string) IsntBufferToHeatBySolar()
        {
            var solar = StateService.GetSensor(SensorName.SolarCollector).Value;
            var bufferBottom = StateService.GetSensor(SensorName.BufferBottom).Value;
            if (solar < (bufferBottom + 3.0))
            {
                return (true, $"Solar ({solar}) is not hot enough to heat buffer: {bufferBottom})");
            }
            return (false, "");
        }
        
        #endregion


        #region Aktionen

        async void DoAllOff(object sender, EventArgs e)
        {
            Log.Information($"Fsm;HotWater;DoAllOff");
            var pumpBoiler = StateService.GetActor(ActorName.PumpBoiler);
            var pumpSolar = StateService.GetActor(ActorName.PumpSolar);
            var valveBoilerBuffer = StateService.GetActor(ActorName.ValveBoilerBuffer);
            await SerialCommunicationService.SetActorAsync(pumpBoiler.Name, 0);
            await SerialCommunicationService.SetActorAsync(pumpSolar.Name, 0);
            await SerialCommunicationService.SetActorAsync(valveBoilerBuffer.Name, 0);
            OilBurner.IsBurnerNeededByHotWater = false;
        }

        async void DoHeatBoilerByBurner(object sender, EventArgs e)
        {
            Log.Information($"Fsm;HotWater;DoHeatBoilerByBurner");
            var pumpBoiler = StateService.GetActor(ActorName.PumpBoiler);
            var pumpSolar = StateService.GetActor(ActorName.PumpSolar);
            var valveBoilerBuffer = StateService.GetActor(ActorName.ValveBoilerBuffer);
            await SerialCommunicationService.SetActorAsync(pumpBoiler.Name, 1);
            await SerialCommunicationService.SetActorAsync(pumpSolar.Name, 0);
            await SerialCommunicationService.SetActorAsync(valveBoilerBuffer.Name, 0);
            OilBurner.IsBurnerNeededByHotWater = true;
        }

        async void DoHeatBoilerBySolar(object sender, EventArgs e)
        {
            Log.Information($"Fsm;HotWater;DoHeatBoilerBySolar");
            var pumpBoiler = StateService.GetActor(ActorName.PumpBoiler);
            var pumpSolar = StateService.GetActor(ActorName.PumpSolar);
            var valveBoilerBuffer = StateService.GetActor(ActorName.ValveBoilerBuffer);
            await SerialCommunicationService.SetActorAsync(pumpBoiler.Name, 0);
            await SerialCommunicationService.SetActorAsync(pumpSolar.Name, 1);
            await SerialCommunicationService.SetActorAsync(valveBoilerBuffer.Name, 0);
        }

        async void DoHeatBufferBySolar(object sender, EventArgs e)
        {
            Log.Information($"Fsm;HotWater;DoHeatBufferBySolar");
            var pumpBoiler = StateService.GetActor(ActorName.PumpBoiler);
            var pumpSolar = StateService.GetActor(ActorName.PumpSolar);
            var valveBoilerBuffer = StateService.GetActor(ActorName.ValveBoilerBuffer);
            await SerialCommunicationService.SetActorAsync(pumpBoiler.Name, 0);
            await SerialCommunicationService.SetActorAsync(pumpSolar.Name, 1);
            await SerialCommunicationService.SetActorAsync(valveBoilerBuffer.Name, 1);
        }

        #endregion

    }
}
