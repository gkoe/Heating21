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
    public sealed class OilBurner
    {
        const double BURNER_TOO_HOT = 80.0;
        const double BURNER_READY = 50.0;
        const double BURNER_COOLED_DOWN = 75.0;

        public enum State {  Off, Cold, Ready, TooHot};
        public enum Input { IsNeededOilBurner, IsntNeededOilBurner, IsCold, IsCooledDown, IsReady, IsTooHot  };
        public IStateService StateService { get; }
        public FiniteStateMachine Fsm { get; set; }
        public ISerialCommunicationService SerialCommunicationService { get; }
        public bool IsBurnerNeededByHotWater { get; internal set; }
        public bool IsBurnerNeededByHeatingCircuit { get; internal set; }

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
                Fsm.GetInput(Input.IsNeededOilBurner).TriggerMethod = IsNeededOilBurner;
                Fsm.GetInput(Input.IsntNeededOilBurner).TriggerMethod = IsntNeededOilBurner;
                Fsm.GetInput(Input.IsCold).TriggerMethod = IsCold;
                Fsm.GetInput(Input.IsCooledDown).TriggerMethod = IsCooledDown;
                Fsm.GetInput(Input.IsReady).TriggerMethod = IsReady;
                Fsm.GetInput(Input.IsTooHot).TriggerMethod = IsTooHot;
                // Übergänge definieren
                Fsm.AddTransition(State.Off, State.Cold, Input.IsNeededOilBurner);
                Fsm.AddTransition(State.Cold, State.Ready, Input.IsReady);
                Fsm.AddTransition(State.Cold, State.Off, Input.IsntNeededOilBurner);
                Fsm.AddTransition(State.Ready, State.Cold, Input.IsCold);
                Fsm.AddTransition(State.Ready, State.TooHot, Input.IsTooHot);
                Fsm.AddTransition(State.Ready, State.Off, Input.IsntNeededOilBurner);
                Fsm.AddTransition(State.TooHot, State.Ready, Input.IsCooledDown);
                // Aktionen festlegen
                Fsm.GetState(State.Off).OnLeave += DoBurnerOn;
                Fsm.GetState(State.Off).OnEnter += DoBurnerOff;
            }
            catch (Exception ex)
            {
                Log.Error($"Fehler bei Init FsmOilBurner, ex: {ex.Message}");
            }
        }
        #region TriggerMethoden

        /// <summary>
        /// Ist der Ölbrenner zu heiß?
        /// </summary>
        /// <returns></returns>
        public bool IsTooHot() => StateService.GetSensor(ItemEnum.OilBurnerTemperature).Value >= BURNER_TOO_HOT; 
        public bool IsCold() => StateService.GetSensor(ItemEnum.OilBurnerTemperature).Value < 27.0; //500;

        /// <summary>
        /// Abschalttemperatur erreicht
        /// </summary>
        /// <returns></returns>
        public bool IsReady()
        {
            var temp = StateService.GetSensor(ItemEnum.OilBurnerTemperature).Value;
            return temp >= BURNER_READY;
        }

        /// <summary>
        /// Ist Ölkessel abgekühlt
        /// </summary>
        /// <returns></returns>
        public bool IsCooledDown()
        {
            return StateService.GetSensor(ItemEnum.OilBurnerTemperature).Value <= BURNER_COOLED_DOWN;
        }


        private bool IsNeededOilBurner()
        {
            return IsBurnerNeededByHeatingCircuit || IsBurnerNeededByHotWater;
        }

        private bool IsntNeededOilBurner()
        {
            return !IsNeededOilBurner();
        }
        #endregion


        #region Aktionen
        async void DoBurnerOn(object sender, EventArgs e)
        {
            var oilBurnerSwitch = StateService.GetActor(ItemEnum.OilBurnerSwitch);
            await SerialCommunicationService.SetActorAsync(oilBurnerSwitch.ItemName.ToString(), 1);
        }

        async void DoBurnerOff(object sender, EventArgs e)
        {
            var oilBurnerSwitch = StateService.GetActor(ItemEnum.OilBurnerSwitch);
            await SerialCommunicationService.SetActorAsync(oilBurnerSwitch.ItemName.ToString(), 0);
        }
        #endregion

    }
}
