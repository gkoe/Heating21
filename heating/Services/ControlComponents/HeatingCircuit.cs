using HeatControl.Fsm;
using Serilog;
using Services.Contracts;
using Services.DataTransferObjects;
using Services.Fsm;
using System;
using System.Linq;

namespace Services.ControlComponents
{
    public sealed class HeatingCircuit
    {
        const double OG_TEMP = 23.5;

        public enum State { Off, PumpIsOff, WaitBurnerReadyToHeat, PumpIsOn, CoolBurnerByCircuit };
        public enum Input
        {
            IsInHeatingTime, IsntInHeatingTime, IsBurnerToCool, IsntBurnerToCool, IsBurnerReady,
            IsBurnerCold, IsHot, IsCold
        };
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
                Fsm.GetInput(Input.IsBurnerReady).TriggerMethod = IsBurnerReady;
                Fsm.GetInput(Input.IsBurnerCold).TriggerMethod = IsBurnerCold;
                Fsm.GetInput(Input.IsHot).TriggerMethod = IsHot;
                Fsm.GetInput(Input.IsCold).TriggerMethod = IsCold;
                // Übergänge definieren
                Fsm.AddTransition(State.Off, State.PumpIsOff, Input.IsInHeatingTime);
                Fsm.AddTransition(State.Off, State.CoolBurnerByCircuit, Input.IsBurnerToCool);
                Fsm.AddTransition(State.CoolBurnerByCircuit, State.Off, Input.IsntBurnerToCool);
                Fsm.AddTransition(State.PumpIsOff, State.Off, Input.IsntInHeatingTime);
                Fsm.AddTransition(State.PumpIsOff, State.CoolBurnerByCircuit, Input.IsBurnerToCool);
                Fsm.AddTransition(State.PumpIsOff, State.WaitBurnerReadyToHeat, Input.IsCold)
                    .OnSelect += Select_Transition_PumpIsOff_WaitBurnerReadyToHeat_IsCold;
                Fsm.AddTransition(State.WaitBurnerReadyToHeat, State.PumpIsOn, Input.IsBurnerReady);
                Fsm.AddTransition(State.PumpIsOn, State.PumpIsOff, Input.IsBurnerCold);
                Fsm.AddTransition(State.PumpIsOn, State.PumpIsOff, Input.IsHot)
                    .OnSelect += Select_Transition_PumpIsOn_PumpIsOff_IsHot; ;
                // Aktionen festlegen
                Fsm.GetState(State.PumpIsOn).OnEnter += DoPumpOn;
                Fsm.GetState(State.PumpIsOff).OnEnter += DoPumpOff;
                Fsm.GetState(State.CoolBurnerByCircuit).OnEnter += DoPumpOn;
            }
            catch (Exception ex)
            {
                Log.Error($"HeatingCircuit;Fehler bei Init FsmHeatingCircuit, ex: {ex.Message}");
            }
        }

        #region TriggerMethoden

        public (bool, string) IsBurnerCold() => OilBurner.IsCold();
        private (bool, string) IsBurnerTooHot() => OilBurner.IsTooHot();
        public (bool, string) IsCooledDown() => OilBurner.IsCooledDown();

        private (bool, string) IsHot()
        {
            var temperature = StateService.GetSensor(ItemEnum.LivingroomFirstFloor).Value;
            bool isHot = temperature >= OG_TEMP;
            return (isHot, $"LivingRoomTemperature: {temperature}");
        }

        private (bool, string) IsCold()
        {
            var temperature = StateService.GetSensor(ItemEnum.LivingroomFirstFloor).Value;
            bool isCold = temperature <= OG_TEMP -0.5;
            return (isCold, $"LivingRoomTemperature: {temperature}");
        }

        private (bool, string) IsBurnerReady() => OilBurner.IsReady();

        private readonly TriggerMethod IsInHeatingTime = () => (DateTime.Now.Hour >= 6 && DateTime.Now.Hour <= 21, "");

        private (bool, string) IsntInHeatingTime()
        {
            return (!IsInHeatingTime().IsTriggered, "");
        }
        #endregion

        #region Triggermethoden Transitionen

        private void Select_Transition_PumpIsOff_WaitBurnerReadyToHeat_IsCold(object sender, EventArgs e)
        {
            OilBurner.IsBurnerNeededByHeatingCircuit = true;
        }

        private void Select_Transition_PumpIsOn_PumpIsOff_IsHot(object sender, EventArgs e)
        {
            OilBurner.IsBurnerNeededByHeatingCircuit = false;
        }
        #endregion

        #region Aktionen
        async void DoPumpOn(object sender, EventArgs e)
        {
            var pumpFirstFloorSwitch = StateService.GetActor(ItemEnum.PumpFirstFloor);
            await SerialCommunicationService.SetActorAsync(pumpFirstFloorSwitch.ItemName.ToString(), 1);
            var pumpGroundFloorSwitch = StateService.GetActor(ItemEnum.PumpGroundFloor);
            await SerialCommunicationService.SetActorAsync(pumpGroundFloorSwitch.ItemName.ToString(), 1);
        }

        async void DoPumpOff(object sender, EventArgs e)
        {
            //OilBurner.IsBurnerNeededByHeatingCircuit = false;
            var pumpFirstFloorSwitch = StateService.GetActor(ItemEnum.PumpFirstFloor);
            await SerialCommunicationService.SetActorAsync(pumpFirstFloorSwitch.ItemName.ToString(), 0);
            var pumpGroundFloorSwitch = StateService.GetActor(ItemEnum.PumpGroundFloor);
            await SerialCommunicationService.SetActorAsync(pumpGroundFloorSwitch.ItemName.ToString(), 0);
        }

        //void OilBurnerNeeded(object sender, EventArgs e)
        //{
        //    OilBurner.IsBurnerNeededByHeatingCircuit = true;
        //}
        #endregion

    }
}
