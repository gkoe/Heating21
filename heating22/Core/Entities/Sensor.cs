namespace Core.Entities
{

    /// <summary>
    /// Sensoren werden über die Enum SensorName typsicher benannt.
    /// </summary>
    public class Sensor : Item
    {
        public Sensor(ItemEnum itemEnum, string unit, int persistenceInterval = 900) : base(itemEnum, unit, persistenceInterval)
        {
        }

        public Sensor()  { }

        public override string ToString() => $"Sensor: {Name}";

    }
}
