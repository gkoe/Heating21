using Serilog;

using Services.Contracts;
using Services.DataTransferObjects;
using Services.Fsm;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.ControlComponents
{
    public sealed class HeatingCircuit
    {
        public enum State {  Off, PumpIsOff, PumpIsOn, CoolBurner};
        public enum Input { IsInHeatingTime, IsntInHeatingTime, IsBurnerToCool, IsntBurnerToCool, IsColdAndBurnerReady, IsBurnerCold, IsHot  };
        public IStateService StateService { get; }
        public FiniteStateMachine Fsm { get; set; }
        public OilBurner OilBurner { get; }
        public ISerialCommunicationService SerialCommunicationService { get; set; }

        public HeatingCircuit(IStateService stateService, ISerialCommunicationService serialCommunicationService, OilBurner oilBurner)
        {
            SerialCommunicationService = serialCommunicationService;
            OilBurner = oilBurner;
            StateService = stateService;
            InitFsm();
        }

        public void Start()
        {
            Fsm.Start(State.Off);
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
            Fsm = new FiniteStateMachine(nameof(HeatingCircuit), stateEnums, inputEnums);
            try
            {
                // Triggermethoden bei Inputs definieren
                Fsm.GetInput(Input.IsInHeatingTime).TriggerMethod = IsInHeatingTime;
                Fsm.GetInput(Input.IsntInHeatingTime).TriggerMethod = IsntInHeatingTime;
                Fsm.GetInput(Input.IsBurnerToCool).TriggerMethod = IsBurnerTooHot;
                Fsm.GetInput(Input.IsntBurnerToCool).TriggerMethod = IsCooledDown;
                Fsm.GetInput(Input.IsColdAndBurnerReady).TriggerMethod = IsColdAndBurnerReady;
                Fsm.GetInput(Input.IsBurnerCold).TriggerMethod = IsBurnerCold;
                Fsm.GetInput(Input.IsHot).TriggerMethod = IsHot;
                // Übergänge definieren
                Fsm.AddTransition(State.Off, State.PumpIsOff, Input.IsInHeatingTime);
                Fsm.AddTransition(State.Off, State.CoolBurner, Input.IsBurnerToCool);
                Fsm.AddTransition(State.CoolBurner, State.Off, Input.IsntBurnerToCool);
                Fsm.AddTransition(State.PumpIsOff, State.Off, Input.IsntInHeatingTime);
                Fsm.AddTransition(State.PumpIsOff, State.CoolBurner, Input.IsBurnerToCool);
                Fsm.AddTransition(State.PumpIsOff, State.PumpIsOn, Input.IsColdAndBurnerReady);
                Fsm.AddTransition(State.PumpIsOn, State.PumpIsOff, Input.IsBurnerCold);
                Fsm.AddTransition(State.PumpIsOn, State.PumpIsOff, Input.IsHot);
                // Aktionen festlegen
                Fsm.GetState(State.PumpIsOn).OnEnter += DoBurnerOn;
                Fsm.GetState(State.PumpIsOn).OnLeave += DoBurnerOff;
                Fsm.GetState(State.CoolBurner).OnEnter += DoBurnerOn;
                Fsm.GetState(State.CoolBurner).OnLeave += DoBurnerOff;
            }
            catch (Exception ex)
            {
                Log.Error("Fehler bei Init FsmHeatingCircuit");
            }
        }
        #region TriggerMethoden

        public bool IsBurnerCold() => OilBurner.IsCold();
        private bool IsBurnerTooHot() => OilBurner.IsTooHot();
        public bool IsCooledDown() => OilBurner.IsCooledDown();

        private bool IsHot() => StateService.GetSensor(ItemEnum.LivingroomFirstFloor).Value >= 23.5;

        private bool IsColdAndBurnerReady()
        {
            if (!OilBurner.IsReady())
            {
                return false;
            }
            var temp = StateService.GetSensor(ItemEnum.LivingroomFirstFloor).Value;
            return  temp < 23.0;
        }


        private bool IsInHeatingTime()
        {
            //bool isNeeded = MainControl.Instance.IsBurnerNeeded();
            return true;
        }

        private bool IsntInHeatingTime()
        {
            return !IsInHeatingTime();
        }
        #endregion


        #region Aktionen
        async void DoBurnerOn(object sender, EventArgs e)
        {
            var pumpFirstFloorSwitch = StateService.GetActor(ItemEnum.PumpFirstFloor);
            await SerialCommunicationService.SetActorAsync(pumpFirstFloorSwitch.ItemEnum.ToString(), 1);
        }

        async void DoBurnerOff(object sender, EventArgs e)
        {
            var pumpFirstFloorSwitch = StateService.GetActor(ItemEnum.PumpFirstFloor);
            await SerialCommunicationService.SetActorAsync(pumpFirstFloorSwitch.ItemEnum.ToString(), 0);
        }
        #endregion

    }
}
