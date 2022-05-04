using System;
using System.Collections;

using Base.Helper;

using Serilog;

using Services.Fsm;

namespace HeatControl.Fsm
{
    public class State
    {

        public Enum StateEnum { get; private set; }
        public FiniteStateMachine Fsm { get; private set; }
        public bool IsEndState { get; set; }
        public ArrayList Transitions { get; set; }
        public event EventHandler? OnEnter;
        public event EventHandler? OnLeave;

        public State(FiniteStateMachine fsm, Enum stateEnum, EventHandler? onEnter = null, EventHandler? onLeave = null)
        {
            StateEnum = stateEnum;
            Fsm = fsm;
            Transitions = new ArrayList();
            IsEndState = false;
            if (onEnter != null)
            {
                OnEnter += onEnter;
            }
            if (onLeave != null)
            {
                OnLeave += onLeave;
            }
        }

        public void Leave()
        {
            //Log.Information($"Fsm: {Fsm} State {StateEnum};Leave");
            OnLeave?.Invoke(this, new());
        }

        public void Enter()
        {
            //Log.Information($"Fsm: {Fsm} State {StateEnum};Enter");
            OnEnter?.Invoke(this, new());
        }

        /// <summary>
        /// Transition für Input suchen
        /// </summary>
        /// <param name="inputNumber"></param>
        /// <returns>Transition oder null</returns>
        public Transition? GetTransitionByInput(Enum input)
        {
            foreach (Transition transition in Transitions)
            {
                if (EnumHelper.ToInt(transition.Input.InputEnum) == EnumHelper.ToInt(input))
                {
                    return transition;
                }
            }
            return null;
        }

        public override string ToString()
        {
            return $"Fsm: {Fsm}, State: {StateEnum}";
        }

    }
}
