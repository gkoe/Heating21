
using Core.Entities;

using IotServices.Services;

using Serilog;

using Services.Contracts;
using Services.Fsm;

using System;
using System.Linq;

namespace Services.ControlComponents
{
    public sealed class OilBurner
    {
        private static readonly Lazy<OilBurner> lazy = new(() => new OilBurner());
        public static OilBurner Instance { get { return lazy.Value; } }

        public static double TargetTemperature 
        {
            get
            {
                var livingRoomTemperature = StateService.Instance.GetSensor(ItemEnum.HmoLivingroomFirstFloor).Value;
                var deltaRoomTemperature = HeatingCircuit.Instance.TargetTemperature - livingRoomTemperature;
                var temperatureSurcharge = Math.Min(deltaRoomTemperature / 5.0 , 1) * 15;  // kann nicht über 15° erhöhen
                return BURNER_HOT + temperatureSurcharge; // 16° ==> 60+1*15 = 75; 21,5° 60+1/5*15=63
            }
        }

        const double BURNER_COLD = 30.0;
        const double BURNER_READY = 50.0;
        const double BURNER_HOT = 60.0;
        const double BURNER_TOO_HOT = 80.0;
        const double BURNER_COOLED_HOT = 75.0;

       

        public enum State {  Off, Cold, Ready, Hot, TooHot};
        public enum Input { IsNeededOilBurner, IsntNeededOilBurner, IsCooledToCold, IsHeatedToReady, IsCooledToReady, IsHeatedToHot, 
                            IsCooledToHot, IsHeatedToTooHot };
        public FiniteStateMachine Fsm { get; set; }

        public bool IsBurnerNeededByHotWater { get; internal set; }
        public bool IsBurnerNeededByHeatingCircuit { get; internal set; }

        public bool IsBurnerReadyToHeat => (State)Fsm.ActState.StateEnum == State.Ready
                                            || (State)Fsm.ActState.StateEnum == State.Hot
                                            || (State)Fsm.ActState.StateEnum == State.TooHot;

        public OilBurner()
        {
            Enum[] stateEnums = Enum.GetValues<State>().Select(e => (Enum)e).ToArray();
            Enum[] inputEnums = Enum.GetValues<Input>().Select(e => (Enum)e).ToArray();
            Fsm = new FiniteStateMachine(nameof(OilBurner), stateEnums, inputEnums);
            InitFsm();
        }

        //public OilBurner(IStateService stateService, ISerialCommunicationService serialCommunicationService) : this(stateService)
        //{
        //    SerialCommunicationService = serialCommunicationService;
        //}

        public void Start()
        {
            Fsm.Start(State.Off);
        }

        public void Stop()
        {
            Fsm.Stop();
        }

        public static double GetTemperature()
        {
            var temperature = StateService.Instance.GetSensor(ItemEnum.OilBurnerTemperature).Value;
            return temperature;
        }

        /// <summary>
        /// Fsm wird über Enums angelegt
        /// </summary>
        void InitFsm()
        {

            //string[] stateNames = Enum.GetNames(typeof(State));
            //string[] inputNames = Enum.GetNames(typeof(Input));
            try
            {
                // Triggermethoden bei Inputs definieren
                var inputIsNeededOilBurner = Fsm.GetInput(Input.IsNeededOilBurner);
                inputIsNeededOilBurner.TriggerMethod = IsNeededOilBurner;
                inputIsNeededOilBurner.OnInput += DoBurnerOn;
                var inputIsntNeededOilBurner = Fsm.GetInput(Input.IsntNeededOilBurner);
                inputIsntNeededOilBurner .TriggerMethod = IsntNeededOilBurner;
                inputIsntNeededOilBurner.OnInput += DoBurnerOff;
                Fsm.GetInput(Input.IsCooledToCold).TriggerMethod = IsCooledToCold;
                Fsm.GetInput(Input.IsCooledToHot).TriggerMethod = IsCooledToHot;
                Fsm.GetInput(Input.IsHeatedToReady).TriggerMethod = IsHeatedToReady;
                Fsm.GetInput(Input.IsCooledToReady).TriggerMethod = IsCooledToReady;
                Fsm.GetInput(Input.IsHeatedToHot).TriggerMethod = IsHeatedToHot;
                Fsm.GetInput(Input.IsCooledToHot).TriggerMethod = IsCooledToHot;
                Fsm.GetInput(Input.IsHeatedToTooHot).TriggerMethod = IsHeatedToTooHot;
                // Übergänge definieren
                Fsm.AddTransition(State.Off, State.Cold, Input.IsNeededOilBurner);
                Fsm.AddTransition(State.Cold, State.Ready, Input.IsHeatedToReady);
                Fsm.AddTransition(State.Cold, State.Off, Input.IsntNeededOilBurner);
                Fsm.AddTransition(State.Ready, State.Off, Input.IsCooledToCold);
                Fsm.AddTransition(State.Ready, State.TooHot, Input.IsHeatedToTooHot);
                Fsm.AddTransition(State.Ready, State.Off, Input.IsntNeededOilBurner);
                Fsm.AddTransition(State.TooHot, State.Ready, Input.IsCooledToHot);
                // Aktionen festlegen
                //Fsm.GetState(State.Off).OnLeave += DoBurnerOn;
                //Fsm.GetState(State.Off).OnEnter += DoBurnerOff;
                Fsm.GetState(State.TooHot).OnEnter += DoBurnerOff;
            }
            catch (Exception ex)
            {
                Log.Error($"Fehler bei Init FsmOilBurner, ex: {ex.Message}");
            }
        }

        #region TriggerMethoden

        public static (bool, string) IsCooledToCold()
        {
            var temperature = StateService.Instance.GetSensor(ItemEnum.OilBurnerTemperature).Value;
            bool isCold = temperature <= BURNER_COLD;
            return (isCold, $"OilBurnerTemperature: {temperature}");
        }

        /// <summary>
        /// Abschalttemperatur erreicht
        /// </summary>
        /// <returns></returns>
        public static (bool, string) IsHeatedToReady()
        {
            var temperature = StateService.Instance.GetSensor(ItemEnum.OilBurnerTemperature).Value;
            var isReady = temperature >= BURNER_READY;
            return (isReady, $"OilBurnerTemperature: {temperature}");
        }

        /// <summary>
        /// Abschalttemperatur erreicht
        /// </summary>
        /// <returns></returns>
        public static (bool, string) IsCooledToReady()
        {
            var temperature = StateService.Instance.GetSensor(ItemEnum.OilBurnerTemperature).Value;
            var isReady = temperature <= BURNER_READY;
            return (isReady, $"OilBurnerTemperature: {temperature}");
        }

        public static (bool, string) IsHeatedToHot()
        {
            var temperature = StateService.Instance.GetSensor(ItemEnum.OilBurnerTemperature).Value;
            bool isTooHot = temperature >= TargetTemperature; // BURNER_HOT;
            return (isTooHot, $"OilBurnerTemperature: {temperature}");
        }

        public static (bool, string) IsCooledToHot()
        {
            var temperature = StateService.Instance.GetSensor(ItemEnum.OilBurnerTemperature).Value;
            bool isTooHot = temperature <= BURNER_COOLED_HOT;
            return (isTooHot, $"OilBurnerTemperature: {temperature}");
        }

        /// <summary>
        /// Ist der Ölbrenner zu heiß?
        /// </summary>
        /// <returns></returns>
        public static (bool, string) IsHeatedToTooHot()
        {
            var temperature = StateService.Instance.GetSensor(ItemEnum.OilBurnerTemperature).Value;
            bool isTooHot = temperature >= BURNER_TOO_HOT;
            return (isTooHot, $"OilBurnerTemperature: {temperature}");
        }

        private (bool,string) IsNeededOilBurner()
        {
            var temperature = StateService.Instance.GetSensor(ItemEnum.OilBurnerTemperature).Value;
            if (temperature >= BURNER_TOO_HOT)
            {
                return (false, "");
            }
            if (IsBurnerNeededByHeatingCircuit && IsBurnerNeededByHotWater)
            {
                return (true, $"IsBurnerNeededByHeatingCircuit && IsBurnerNeededByHotWater, Oilburner: {temperature}");
            }
            if (IsBurnerNeededByHeatingCircuit)
            {
                return (true, $"IsBurnerNeededByHeatingCircuit, Oilburner: {temperature}");
            }
            if (IsBurnerNeededByHotWater)
            {
                return (true, $"IsBurnerNeededByHotWater, Oilburner: {temperature}");
            }
            return (false, "");
        }

        private (bool,string) IsntNeededOilBurner()
        {
            if (!IsBurnerNeededByHeatingCircuit && !IsBurnerNeededByHotWater)
            {
                return (true, "!IsBurnerNeededByHeatingCircuit && !IsBurnerNeededByHotWater");
            }
            return (false, "");
        }
        #endregion


        #region Aktionen
        void DoBurnerOn(object? sender, EventArgs? e)
        {
            Log.Information($"Fsm;OilBurner;DoBurnerOn");
            var oilBurnerSwitch = StateService.Instance.GetActor(ItemEnum.OilBurnerSwitch);
            SerialCommunicationService.Instance.SetActorAsync(oilBurnerSwitch.Name, 1).Wait();
        }

        void DoBurnerOff(object? sender, EventArgs? e)
        {
            Log.Information($"Fsm;OilBurner;DoBurnerOff");
            var oilBurnerSwitch = StateService.Instance.GetActor(ItemEnum.OilBurnerSwitch);
            SerialCommunicationService.Instance.SetActorAsync(oilBurnerSwitch.Name, 0).Wait();
        }
        #endregion

        #region Sicherheitschecks
        public void CheckOilBurner()
        {
            var (needed, _) =IsNeededOilBurner();
            var oilBurnerSwitch = StateService.Instance.GetActor(ItemEnum.OilBurnerSwitch);
            if (needed && oilBurnerSwitch.Value == 0)
            {
                Log.Warning($"Fsm;OilBurner;Burner should be on, State: {Fsm.ActState.StateEnum}");
                DoBurnerOn(this, null);
                return;
            }
            var (notNeeded, _) = IsntNeededOilBurner();
            if (notNeeded && oilBurnerSwitch.Value == 1)
            {
                Log.Warning($"Fsm;OilBurner;Burner should be off, State: {Fsm.ActState.StateEnum}");
                DoBurnerOff(this, null);
                return;
            }
        }
        #endregion


    }
}
