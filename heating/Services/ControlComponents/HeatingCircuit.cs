
using Base.Helper;

using Core.Entities;

using HeatControl.Fsm;

using Serilog;

using Services.Contracts;
using Services.Fsm;

using System;
using System.Linq;

namespace Services.ControlComponents
{
    public sealed class HeatingCircuit
    {
        public double TargetTemperature { get; set; } = 22.5;

        public enum State { Off, PumpIsOff, WaitBurnerReadyToHeat, PumpIsOn, UseResidualHeat, CoolBurnerByCircuit };
        public enum Input
        {
            IsInHeatingTime, IsntInHeatingTime, IsBurnerToCool, IsntBurnerToCool, IsBurnerReadyToHeat,
            IsntBurnerReadyToHeat, IsHot, IsCold, IsAllResidualHeatUsed
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
                Fsm.GetInput(Input.IsBurnerReadyToHeat).TriggerMethod = IsBurnerReady;
                Fsm.GetInput(Input.IsntBurnerReadyToHeat).TriggerMethod = IsBurnerCold;
                Fsm.GetInput(Input.IsHot).TriggerMethod = IsHot;
                Fsm.GetInput(Input.IsCold).TriggerMethod = IsCold;
                Fsm.GetInput(Input.IsAllResidualHeatUsed).TriggerMethod = IsAllResidualHeatUsed;
                // Übergänge definieren
                Fsm.AddTransition(State.Off, State.PumpIsOff, Input.IsInHeatingTime);
                Fsm.AddTransition(State.Off, State.CoolBurnerByCircuit, Input.IsBurnerToCool);
                Fsm.AddTransition(State.CoolBurnerByCircuit, State.PumpIsOff, Input.IsntBurnerToCool);
                Fsm.AddTransition(State.PumpIsOff, State.Off, Input.IsntInHeatingTime);
                Fsm.AddTransition(State.PumpIsOff, State.PumpIsOn, Input.IsCold)
                    .OnSelect += SwitchOilBurnerOn;
                Fsm.AddTransition(State.PumpIsOff, State.CoolBurnerByCircuit, Input.IsBurnerToCool);
                //Fsm.AddTransition(State.WaitBurnerReadyToHeat, State.PumpIsOn, Input.IsBurnerReadyToHeat);
                Fsm.AddTransition(State.PumpIsOn, State.PumpIsOff, Input.IsntInHeatingTime)
                    .OnSelect += SwitchOilBurnerOff; ;
                Fsm.AddTransition(State.PumpIsOn, State.UseResidualHeat, Input.IsHot)
                    .OnSelect += SwitchOilBurnerOff; ;
                Fsm.AddTransition(State.UseResidualHeat, State.PumpIsOff, Input.IsAllResidualHeatUsed);
                // Aktionen festlegen
                Fsm.GetState(State.PumpIsOn).OnEnter += DoPumpOn;
                Fsm.GetState(State.CoolBurnerByCircuit).OnEnter += DoPumpOn;
                Fsm.GetState(State.PumpIsOff).OnEnter += DoPumpOff;
            }
            catch (Exception ex)
            {
                Log.Error($"HeatingCircuit;Fehler bei Init FsmHeatingCircuit, ex: {ex.Message}");
            }
        }

        #region TriggerMethoden

        public (bool, string) IsBurnerCold() => OilBurner.IsCooledToCold();
        private (bool, string) IsBurnerTooHot() => OilBurner.IsHeatedToTooHot();
        public (bool, string) IsCooledDown() => OilBurner.IsCooledToHot();

        private (bool, string) IsHot()
        {
            var temperature = StateService.GetSensor(SensorName.HmoLivingroomFirstFloor).Value;
            bool isHot = temperature >= TargetTemperature-0.2;  // Heizung läuft sonst weit drüber
            return (isHot, $"LivingRoomTemperature: {temperature}");
        }

        private (bool, string) IsCold()
        {
            var temperature = StateService.GetSensor(SensorName.HmoLivingroomFirstFloor).Value;
            bool isCold = temperature <= TargetTemperature - 0.4;
            return (isCold, $"LivingRoomTemperature: {temperature}");
        }

        private (bool, string) IsAllResidualHeatUsed()
        {
            if(OilBurner.IsBurnerNeededByHotWater)
            {
                return (true, "No use of residual heat, because burner is used by HotWater");
            }
            var (isOilBurnerCooled, message) = OilBurner.IsCooledToReady();
            var temperature = StateService.GetSensor(SensorName.HmoLivingroomFirstFloor).Value;
            return (isOilBurnerCooled, $"IsAllResidualHeatUsed: LivingRoom: {temperature}, {message}");
        }

        private (bool, string) IsBurnerReady() => OilBurner.IsHeatedToReady();

        private readonly TriggerMethod IsInHeatingTime = () => (DateTime.Now.Hour >= 6 && DateTime.Now.Hour <= 21, "");

        private (bool, string) IsntInHeatingTime()
        {
            return (!IsInHeatingTime().IsTriggered, "");
        }
        #endregion

        #region Triggermethoden Transitionen

        private void SwitchOilBurnerOn(object sender, EventArgs e)
        {
            OilBurner.IsBurnerNeededByHeatingCircuit = true;
        }

        private void SwitchOilBurnerOff(object sender, EventArgs e)
        {
            OilBurner.IsBurnerNeededByHeatingCircuit = false;
        }
        #endregion

        #region Aktionen
        async void DoPumpOn(object sender, EventArgs e)
        {
            Log.Information($"Fsm;HeatingCircuit;DoPumpOn");
            var pumpFirstFloorSwitch = StateService.GetActor(ActorName.PumpFirstFloor);
            await SerialCommunicationService.SetActorAsync(pumpFirstFloorSwitch.Name, 1);
            var pumpGroundFloorSwitch = StateService.GetActor(ActorName.PumpGroundFloor);
            await SerialCommunicationService.SetActorAsync(pumpGroundFloorSwitch.Name, 1);
        }

        async void DoPumpOff(object sender, EventArgs e)
        {
            Log.Information($"Fsm;HeatingCircuit;DoPumpOff");
            //OilBurner.IsBurnerNeededByHeatingCircuit = false;
            var pumpFirstFloorSwitch = StateService.GetActor(ActorName.PumpFirstFloor);
            await SerialCommunicationService.SetActorAsync(pumpFirstFloorSwitch.Name, 0);
            var pumpGroundFloorSwitch = StateService.GetActor(ActorName.PumpGroundFloor);
            await SerialCommunicationService.SetActorAsync(pumpGroundFloorSwitch.Name, 0);
        }

        //void OilBurnerNeeded(object sender, EventArgs e)
        //{
        //    OilBurner.IsBurnerNeededByHeatingCircuit = true;
        //}
        #endregion

        #region Sicherheitschecks
        public void CheckHeating()
        {
            var pumpFirstFloorSwitch = StateService.GetActor(ActorName.PumpFirstFloor);
            var shouldBeOn = EnumHelper.ToInt(Fsm.ActState.StateEnum) == EnumHelper.ToInt(State.CoolBurnerByCircuit)
                || EnumHelper.ToInt(Fsm.ActState.StateEnum) == EnumHelper.ToInt(State.UseResidualHeat)
                || EnumHelper.ToInt(Fsm.ActState.StateEnum) == EnumHelper.ToInt(State.PumpIsOn);
            if (shouldBeOn && pumpFirstFloorSwitch.Value == 0)
            {
                Log.Warning($"Fsm;HeatingCircuit;Pump should be on, State: {Fsm.ActState.StateEnum}");
                DoPumpOn(this, null);
                return;
            }
            if (!shouldBeOn && pumpFirstFloorSwitch.Value == 1)
            {
                Log.Warning($"Fsm;HeatingCircuit;Pump should be off, State: {Fsm.ActState.StateEnum}");
                DoPumpOff(this, null);
                return;
            }
        }
        #endregion


    }
}
