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
        public enum State {  Off, Cold, Ready, TooHot};
        public enum Input { IsNeededOilBurner, IsntNeededOilBurner, IsCold, IsCooledDown, IsReady, IsTooHot  };
        public IStateService StateService { get; }
        public FiniteStateMachine Fsm { get; set; }
        public ISerialCommunicationService SerialCommunicationService { get; }

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
                Log.Error("Fehler bei Init FsmOilBurner");
            }
        }
        #region TriggerMethoden

        /// <summary>
        /// Ist der Ölbrenner zu heiß?
        /// </summary>
        /// <returns></returns>
        public bool IsTooHot() => StateService.GetSensor(ItemEnum.OilBurnerTemperature).Value >= 29.0; // >= 900;
        public bool IsCold() => StateService.GetSensor(ItemEnum.OilBurnerTemperature).Value < 27.0; //500;

        /// <summary>
        /// Abschalttemperatur erreicht
        /// </summary>
        /// <returns></returns>
        public bool IsReady()
        {
            var temp = StateService.GetSensor(ItemEnum.OilBurnerTemperature).Value;
            return  temp >= 27.0; // 500;
        }

        /// <summary>
        /// Ist Ölkessel abgekühlt
        /// </summary>
        /// <returns></returns>
        public bool IsCooledDown()
        {
            return StateService.GetSensor(ItemEnum.OilBurnerTemperature).Value <= 28.0; // 600;
        }


        private bool IsNeededOilBurner()
        {
            //bool isNeeded = MainControl.Instance.IsBurnerNeeded();
            return true;
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
            await SerialCommunicationService.SetActorAsync(oilBurnerSwitch.ItemEnum.ToString(), 1);
        }

        async void DoBurnerOff(object sender, EventArgs e)
        {
            var oilBurnerSwitch = StateService.GetActor(ItemEnum.OilBurnerSwitch);
            await SerialCommunicationService.SetActorAsync(oilBurnerSwitch.ItemEnum.ToString(), 0);
        }
        #endregion

    }
}
