using System;

namespace HeatControl.Fsm
{
    public delegate (bool IsTriggered,string Message) TriggerMethod();

    /// <summary>
    /// Eingangssignal
    /// </summary>
    public class Input
    {
        public Enum InputEnum { get; private set; }
        public TriggerMethod? TriggerMethod { get; set; }
        public event EventHandler? OnInput;

        public Input(Enum inputEnum, TriggerMethod? triggerMethod = null)
        {
            InputEnum = inputEnum;
            if (triggerMethod != null)
            {
                TriggerMethod = triggerMethod;
            }
        }


        public void DoOnInput()
        {
            OnInput?.Invoke(this, new());
        }
    }
}

