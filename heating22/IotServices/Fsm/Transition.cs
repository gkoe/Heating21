using Serilog;

using System;

namespace HeatControl.Fsm
{
    /// <summary>
    /// Zustandsüberführung von einem Zustand in einen  anderen.
    /// Wird durch den entsprechenden Input ausgelöst
    /// </summary>
    public class Transition
    {
        public State FromState { get; private set; }
        public State ToState { get; private set; }
        public Input Input { get; private set; }

        public string InputMessage { get; set; } = string.Empty;

        public event EventHandler? OnSelect;

        public Transition(State fromState, State toState, Input input, EventHandler? onSelect = null)
        {
            FromState = fromState;
            ToState = toState;
            Input = input;
            if (onSelect != null)
            {
                OnSelect += onSelect;
            }
        }

        /// <summary>
        /// Zustandsüberführung wird ausgelöst.
        /// Führt zur Verständigung etwaig angemeldeter Observer
        /// </summary>
        public void Select()
        {
            OnSelect?.Invoke(this, new());  // Aktivitäten ausführen, die im Zuge der Transition definiert sind
            Log.Information($"Fsm Select Transition;{FromState.Fsm.Name}; Input: {Input.InputEnum};  From: {FromState.StateEnum}; to {ToState.StateEnum}; InputMessage: {InputMessage}");
            Input.DoOnInput();  // Aktivitäten ausführen, die bei Auftreten des Inputs auszuführen sind
        }

        public override string ToString()
        {
            return "Transition von "+FromState.StateEnum+" nach "+ToState.StateEnum+" durch "+Input.InputEnum; 
        }
    }
}
