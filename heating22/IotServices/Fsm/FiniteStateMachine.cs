using Base.Helper;

using Core.Entities;

using HeatControl.Fsm;

using Serilog;

using System;
using System.Timers;

namespace Services.Fsm
{



    /// <summary>
    /// Allgemeiner endlicher Automat.
    /// Wird über Konfiguration an die jeweilige Anforderung angepasst. 
    /// States und Inputs werden typsicher als Enum definiert, im FSM aber mit den
    /// Enumtexten und Enumnummern verwaltet (allgemeiner FSM ist unabhängig von speziellen Enums)
    /// </summary>
    public class FiniteStateMachine
    {
        readonly State[] states;
        readonly Input[] inputs;
        private bool _isRunning;

        public State ActState { get; private set; }
        public string Name { get; private set; }
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                Log.Information($"Fsm;IsRunning;set to {value}");
                _isRunning = value;
            }
        }

        public string LastInputMessage { get; set; } = string.Empty;

        public event EventHandler<FsmTransition>? StateChanged;

        //private Enum _actInput;

        /// <summary>
        /// Die Enums für States und Transitions werden übergeben.
        /// Für jede mögliche Ausprägung von State und Transition wird 
        /// ein Objekt angelegt.
        /// </summary>
        /// <param name="stateEnum"></param>
        /// <param name="transitionEnum"></param>
        public FiniteStateMachine(string name, Enum[] stateEnums, Enum[] inputEnums)
        {
            Name = name;
            states = new State[stateEnums.Length];
            for (int i = 0; i < states.Length; i++)
            {
                states[i] = new State(this, stateEnums[i]);
            }
            inputs = new Input[inputEnums.Length];
            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i] = new Input(inputEnums[i]);
            }
            ActState = states[0];
            var timer = new System.Timers.Timer
            {
                Interval = 10000
            };
            timer.Elapsed += OnSecondChanged;
            timer.Start();
        }


        //public State GetState(Enum stateEnum) => states.Single(s => s.StateEnum == stateEnum);
        //public Input GetInput(Enum inputEnum) => inputs.Single(s => s.InputEnum == inputEnum);

        public Input GetInput(Enum inputEnum)
        {
            foreach (var input in inputs)
            {
                if (EnumHelper.ToInt(input.InputEnum) == EnumHelper.ToInt(inputEnum))
                {
                    return input;
                }
            }
            throw new ApplicationException($"GetInput;Fsm: {Name};Input {inputEnum} not found!");
        }

        public State GetState(Enum stateEnum)
        {
            foreach (var state in states)
            {
                if (EnumHelper.ToInt(state.StateEnum) == EnumHelper.ToInt(stateEnum))
                {
                    return state;
                }
            }
            throw new ApplicationException($"GetState;Fsm: {Name};State {stateEnum} not found!");
        }


        public Transition AddTransition(Enum fromState, Enum toState, Enum @input)
        {
            int fromStateIndex = (int)Convert.ChangeType(fromState, typeof(int));
            var transition = new Transition(GetState(fromState), GetState(toState), GetInput(input));
            states[fromStateIndex].Transitions.Add(transition);
            return transition;
        }

        /// <summary>
        /// Alle Sekunden überprüfen, ob etwas zu tun ist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSecondChanged(object? sender, ElapsedEventArgs e)
        {
            if (IsRunning)
            {
                CheckActStateInputs();
                //var x = ActState;
                //var y = _actInput;
            }
        }

        /// <summary>
        /// Überprüft, ob für den aktuellen Zustand zumindest ein InputTrigger
        /// feuert und löst dann die zugehörige Zustandsüberführung aus
        /// </summary>
        public void CheckActStateInputs()
        {
            var lastState = ActState;
            foreach (Transition transition in ActState.Transitions)  // alle aus dem Zustand möglichen Überführungen prüfen
            {
                if (transition.Input.TriggerMethod != null)  // ist für den Input der Transition eine Triggermethode verfügbar
                {
                    var (IsTriggered, Message) = transition.Input.TriggerMethod();
                    if (IsTriggered)  // triggert die Triggermethode
                    {
                        if (HandleInput(transition.Input.InputEnum, Message))
                        {
                            LastInputMessage = Message;
                            transition.InputMessage = Message;
                            StateChanged?.Invoke(this,
                                new FsmTransition
                                {
                                    Time = DateTime.Now,
                                    Fsm = Name,
                                    LastState = lastState.StateEnum.ToString(),
                                    ActState = ActState.StateEnum.ToString(),
                                    Input = transition.Input.InputEnum.ToString(),
                                    InputMessage = Message
                                });
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// In Ausnahmefällen (z.B. Input führt unabhängig vom aktuellen Zustand in immer
        /// den gleichen Folgezustand) State hart setzen
        /// </summary>
        /// <returns></returns>
        public bool SetState(Enum state)
        {
            int stateNumber = EnumHelper.ToInt(state);
            if (stateNumber < 0)
            {
                Log.Error($"Fsm;SetState;{Name}; Statename {state} existiert nicht");
                return false;
            }
            ActState = states[stateNumber];
            return true;
        }

        /// <summary>
        /// Starte den Automaten beim jeweiligen Zustand
        /// </summary>
        /// <param name="state"></param>
        public void Start(Enum stateEnum)
        {
            int stateNumber = EnumHelper.ToInt(stateEnum);
            if (stateNumber < 0)
            {
                Log.Error($"Fsm;Start;{Name}; Statename {stateEnum} existiert nicht");
                return;
            }
            ActState = states[stateNumber];
            StateChanged?.Invoke(this,
                new FsmTransition
                {
                    Time = DateTime.Now,
                    Fsm = Name,
                    LastState = "",
                    ActState = ActState.StateEnum.ToString(),
                    Input = ""
                });
            IsRunning = true;
            CheckActStateInputs();  // gleich beim Start entsprechend reagieren
        }

        /// <summary>
        /// FSM wird angehalten. Ereignisbearbeitung wird ausgesetzt
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
        }

        /// <summary>
        /// FSM wird wieder bei aktuellem Zustand neu gestartet.
        /// </summary>
        public void Resume()
        {
            IsRunning = true;
        }

        /// <summary>
        /// Der Input wird, wenn im aktuellen Zustand möglich, verarbeitet.
        /// Der Folgezustand wird zum aktuellen Zustand 
        /// </summary>
        /// <param name="input">Eingangssignal</param>
        /// <returns>true, wenn die Verarbeitung möglich war</returns>
        public bool HandleInput(Enum input, string inputMessage)
        {
            if (!IsRunning)
            {
                return false; // Input ignorieren
            }
            //_actInput = input;
            var transition = ActState.GetTransitionByInput(input);
            if (transition == null)
            {
                Log.Error($"Fsm;HandleInput;{Name}; {input} im Zustand {ActState}");

                return false;
            }
            transition.InputMessage = inputMessage;
            ActState.Leave();  // aktuellen Zustand verlassen
            transition.Select();  // Etwaige Beobachter des Zustandsübergangs verständigen
            ActState = transition.ToState;
            ActState.Enter(); // Folgezustand aktivieren
            return true;
        }

    }
}
