
using Core.Entities;

using Serilog;

using Services.Contracts;
using Services.Fsm;

using System;
using System.Linq;

namespace Services.ControlComponents
{
    public sealed class OilBurner
    {
        const double BURNER_COLD = 40.0;
        const double BURNER_READY = 50.0;
        const double BURNER_HOT = 65.0;
        const double BURNER_TOO_HOT = 80.0;
        const double BURNER_COOLED_HOT = 78.0;

        public enum State {  Off, Cold, Ready, Hot, TooHot};
        public enum Input { IsNeededOilBurner, IsntNeededOilBurner, IsCooledToCold, IsHeatedToReady, IsCooledToReady, IsHeatedToHot, 
                            IsCooledToHot, IsHeatedToTooHot };
        public IStateService StateService { get; }
        public FiniteStateMachine Fsm { get; set; }
        public ISerialCommunicationService SerialCommunicationService { get; }
        public bool IsBurnerNeededByHotWater { get; internal set; }
        public bool IsBurnerNeededByHeatingCircuit { get; internal set; }

        public bool IsBurnerReadyToHeat => (State)Fsm.ActState.StateEnum == State.Ready
                                            || (State)Fsm.ActState.StateEnum == State.Hot
                                            || (State)Fsm.ActState.StateEnum == State.TooHot;

        public OilBurner(IStateService stateService)
        {
            StateService = stateService;
            InitFsm();
        }

        public OilBurner(IStateService stateService, ISerialCommunicationService serialCommunicationService) : this(stateService)
        {
            SerialCommunicationService = serialCommunicationService;
        }

        public void Start()
        {
            Fsm.Start(State.Off);
        }

        public void Stop()
        {
            Fsm.Stop();
        }

        public double GetTemperature()
        {
            var temperature = StateService.GetSensor(SensorName.OilBurnerTemperature).Value;
            return temperature;
        }

        /// <summary>
        /// Fsm wird über Enums angelegt
        /// </summary>
        void InitFsm()
        {

            //string[] stateNames = Enum.GetNames(typeof(State));
            //string[] inputNames = Enum.GetNames(typeof(Input));
            Enum[] stateEnums = Enum.GetValues<State>().Select(e => (Enum)e).ToArray();
            Enum[] inputEnums = Enum.GetValues<Input>().Select(e => (Enum)e).ToArray();
            Fsm = new FiniteStateMachine(nameof(OilBurner), stateEnums, inputEnums);
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
                //Fsm.GetState(State.TooHot).OnEnter += DoBurnerOff;
            }
            catch (Exception ex)
            {
                Log.Error($"Fehler bei Init FsmOilBurner, ex: {ex.Message}");
            }
        }

        #region TriggerMethoden

        public (bool, string) IsCooledToCold()
        {
            var temperature = StateService.GetSensor(SensorName.OilBurnerTemperature).Value;
            bool isCold = temperature <= BURNER_COLD;
            return (isCold, $"OilBurnerTemperature: {temperature}");
        }

        /// <summary>
        /// Abschalttemperatur erreicht
        /// </summary>
        /// <returns></returns>
        public (bool, string) IsHeatedToReady()
        {
            var temperature = StateService.GetSensor(SensorName.OilBurnerTemperature).Value;
            var isReady = temperature >= BURNER_READY;
            return (isReady, $"OilBurnerTemperature: {temperature}");
        }

        /// <summary>
        /// Abschalttemperatur erreicht
        /// </summary>
        /// <returns></returns>
        public (bool, string) IsCooledToReady()
        {
            var temperature = StateService.GetSensor(SensorName.OilBurnerTemperature).Value;
            var isReady = temperature <= BURNER_READY;
            return (isReady, $"OilBurnerTemperature: {temperature}");
        }

        public (bool, string) IsHeatedToHot()
        {
            var temperature = StateService.GetSensor(SensorName.OilBurnerTemperature).Value;
            bool isTooHot = temperature >= BURNER_HOT;
            return (isTooHot, $"OilBurnerTemperature: {temperature}");
        }

        public (bool, string) IsCooledToHot()
        {
            var temperature = StateService.GetSensor(SensorName.OilBurnerTemperature).Value;
            bool isTooHot = temperature <= BURNER_COOLED_HOT;
            return (isTooHot, $"OilBurnerTemperature: {temperature}");
        }

        /// <summary>
        /// Ist der Ölbrenner zu heiß?
        /// </summary>
        /// <returns></returns>
        public (bool, string) IsHeatedToTooHot()
        {
            var temperature = StateService.GetSensor(SensorName.OilBurnerTemperature).Value;
            bool isTooHot = temperature >= BURNER_TOO_HOT;
            return (isTooHot, $"OilBurnerTemperature: {temperature}");
        }

        private (bool,string) IsNeededOilBurner()
        {
            var temperature = StateService.GetSensor(SensorName.OilBurnerTemperature).Value;
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
        void DoBurnerOn(object sender, EventArgs e)
        {
            Log.Information($"Fsm;OilBurner;DoBurnerOn");
            var oilBurnerSwitch = StateService.GetActor(ActorName.OilBurnerSwitch);
            SerialCommunicationService.SetActorAsync(oilBurnerSwitch.Name, 1).Wait();
        }

        void DoBurnerOff(object sender, EventArgs e)
        {
            Log.Information($"Fsm;OilBurner;DoBurnerOff");
            var oilBurnerSwitch = StateService.GetActor(ActorName.OilBurnerSwitch);
            SerialCommunicationService.SetActorAsync(oilBurnerSwitch.Name, 0).Wait();
        }
        #endregion

    }
}
