using System;

namespace Core.Entities
{
    public class Actor : Item
    {
        public double SettedValue { get; set; }

        Action<Actor, double> SetActorAction { get; set; }

        public Actor(ItemEnum itemEnum, string unit = "", Action<Actor, double> setActorAction = null, int persistenceInterval = 900) : base(itemEnum, unit, persistenceInterval)
        {
            SetActorAction = setActorAction;
        }


        public Actor()  { }

        public override string ToString() => $"Actor: {Name}";

        public void SetActor(double value)
        {
            SetActorAction?.Invoke(this, value);
        }
    }
}
