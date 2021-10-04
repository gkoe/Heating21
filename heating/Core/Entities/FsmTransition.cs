using Base.Entities;
using System;

namespace Core.Entities
{
    public class FsmTransition : EntityObject
    {
        public DateTime Time { get; set; }
        public string Fsm { get; set; }
        public string LastState { get; set; }
        public string ActState { get; set; }
        public string Input { get; set; }

        public override string ToString() => $"Transition: {Time}, Input: {Input}, Fsm: {Fsm}, from {LastState} to {ActState}";

    }
}
