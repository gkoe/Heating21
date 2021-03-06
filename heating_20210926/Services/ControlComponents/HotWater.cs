using Serilog;
using Services.Contracts;
using Services.DataTransferObjects;
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
            IsBoilerToHeatByBurner, IsBoilerHot, IsBurnerToCool, IsntBurnerToCool, IsBoilerToHeatBySolar,
            IsBufferToHeatBySolar, IsBoilerVeryHot, IsntBufferToHeatBySolar, IsntBoilerToHeatBySolar
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
                Fsm.AddTransition(State.AllOff, State.HeatBoilerBySolar, Input.IsBoilerToHeatBySolar);
                Fsm.AddTransition(State.HeatBoilerByBurner, State.CoolBurner, Input.IsBurnerToCool);
                Fsm.AddTransition(State.HeatBoilerByBurner, State.AllOff, Input.IsBoilerHot);
                Fsm.AddTransition(State.HeatBoilerByBurner, State.HeatBoilerBySolar, Input.IsBoilerToHeatBySolar);
                Fsm.AddTransition(State.HeatBoilerBySolar, State.AllOff, Input.IsntBoilerToHeatBySolar);
                Fsm.AddTransition(State.HeatBoilerBySolar, State.HeatBufferBySolar, Input.IsBoilerVeryHot);
                Fsm.AddTransition(State.HeatBoilerBySolar, State.CoolBurner, Input.IsBurnerToCool);
                Fsm.AddTransition(State.HeatBoilerBySolar, State.AllOff, Input.IsntBoilerToHeatBySolar);
                Fsm.AddTransition(State.HeatBufferBySolar, State.AllOff, Input.IsntBufferToHeatBySolar);
                Fsm.AddTransition(State.HeatBufferBySolar, State.CoolBurner, Input.IsBurnerToCool);
                // Aktionen festlegen
                Fsm.GetState(State.HeatBoilerByBurner).OnEnter += DoHeatBoilerByBurner;
                Fsm.GetState(State.AllOff).OnEnter += DoAllOff;
                Fsm.GetState(State.CoolBurner).OnEnter += DoHeatBoilerByBurner;
                Fsm.GetState(State.HeatBoilerBySolar).OnEnter += DoHeatBoilerBySolar;
                Fsm.GetState(State.HeatBufferBySolar).OnEnter += DoHeatBufferBySolar;
            }
            catch (Exception ex)
            {
                Log.Error($"Fehler bei Init FsmHeatingCircuit, ex: {ex.Message}");
            }
        }
        #region TriggerMethoden

        public bool IsBoilerToHeatBySolar()
        {
            var solar = StateService.GetSensor(ItemEnum.SolarCollector).Value;
            var boilerBottom = StateService.GetSensor(ItemEnum.BoilerBottom).Value;
            return solar > (boilerBottom + 10.0) || solar > 80.0;
        }

        public bool IsntBoilerToHeatBySolar()
        {
            var solar = StateService.GetSensor(ItemEnum.SolarCollector).Value;
            var boilerBottom = StateService.GetSensor(ItemEnum.BoilerBottom).Value;
            return solar < boilerBottom + 5.0;
        }

        private bool IsBurnerToCool() => OilBurner.IsTooHot();
        public bool IsntBurnerToCool() => OilBurner.IsCooledDown();

        private bool IsBoilerHot() => StateService.GetSensor(ItemEnum.BoilerTop).Value >= BOILER_HOT;

        private bool IsBoilerToHeatByBurner()
        {
            if (!OilBurner.IsReady())
            {
                return false;
            }
            return StateService.GetSensor(ItemEnum.BoilerTop).Value <= BOILER_COLD;
        }

        private bool IsBoilerVeryHot()
        {
            return StateService.GetSensor(ItemEnum.BoilerTop).Value >= BOILER_VERY_HOT;
        }

        private bool IsntBufferToHeatBySolar()
        {
            var solar = StateService.GetSensor(ItemEnum.SolarCollector).Value;
            var bufferBottom = StateService.GetSensor(ItemEnum.BufferBottom).Value;
            return solar < (bufferBottom + 3.0);
        }
        
        #endregion


        #region Aktionen

        async void DoAllOff(object sender, EventArgs e)
        {
            var pumpBoiler = StateService.GetActor(ItemEnum.PumpBoiler);
            var pumpSolar = StateService.GetActor(ItemEnum.PumpSolar);
            var valveBoilerBuffer = StateService.GetActor(ItemEnum.ValveBoilerBuffer);
            await SerialCommunicationService.SetActorAsync(pumpBoiler.ItemEnum.ToString(), 0);
            await SerialCommunicationService.SetActorAsync(pumpSolar.ItemEnum.ToString(), 0);
            await SerialCommunicationService.SetActorAsync(valveBoilerBuffer.ItemEnum.ToString(), 0);
            OilBurner.IsBurnerNeededByHotWater = false;
        }

        async void DoHeatBoilerByBurner(object sender, EventArgs e)
        {
            var pumpBoiler = StateService.GetActor(ItemEnum.PumpBoiler);
            var pumpSolar = StateService.GetActor(ItemEnum.PumpSolar);
            var valveBoilerBuffer = StateService.GetActor(ItemEnum.ValveBoilerBuffer);
            await SerialCommunicationService.SetActorAsync(pumpBoiler.ItemEnum.ToString(), 1);
            await SerialCommunicationService.SetActorAsync(pumpSolar.ItemEnum.ToString(), 0);
            await SerialCommunicationService.SetActorAsync(valveBoilerBuffer.ItemEnum.ToString(), 0);
            OilBurner.IsBurnerNeededByHotWater = true;
        }

        async void DoHeatBoilerBySolar(object sender, EventArgs e)
        {
            var pumpBoiler = StateService.GetActor(ItemEnum.PumpBoiler);
            var pumpSolar = StateService.GetActor(ItemEnum.PumpSolar);
            var valveBoilerBuffer = StateService.GetActor(ItemEnum.ValveBoilerBuffer);
            await SerialCommunicationService.SetActorAsync(pumpBoiler.ItemEnum.ToString(), 0);
            await SerialCommunicationService.SetActorAsync(pumpSolar.ItemEnum.ToString(), 1);
            await SerialCommunicationService.SetActorAsync(valveBoilerBuffer.ItemEnum.ToString(), 0);
        }

        async void DoHeatBufferBySolar(object sender, EventArgs e)
        {
            var pumpBoiler = StateService.GetActor(ItemEnum.PumpBoiler);
            var pumpSolar = StateService.GetActor(ItemEnum.PumpSolar);
            var valveBoilerBuffer = StateService.GetActor(ItemEnum.ValveBoilerBuffer);
            await SerialCommunicationService.SetActorAsync(pumpBoiler.ItemEnum.ToString(), 0);
            await SerialCommunicationService.SetActorAsync(pumpSolar.ItemEnum.ToString(), 1);
            await SerialCommunicationService.SetActorAsync(valveBoilerBuffer.ItemEnum.ToString(), 1);
        }

        #endregion

    }
}
