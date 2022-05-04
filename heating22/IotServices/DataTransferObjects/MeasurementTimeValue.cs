using Base.ExtensionMethods;

using Core.Entities;

using IotServices.Services;

using Serilog;

using System;

namespace IotServices.DataTransferObjects
{
    public class MeasurementTimeValue
    {
        public Item? Item { get; set; }

        private ItemEnum _itemEnum;
        public ItemEnum ItemEnum
        {
            get { return _itemEnum; }
            set
            {
                if (_itemEnum != value)
                {
                    _itemEnum = value;
                    ItemName = _itemEnum.ToString();
                }
            }
        }
        public string ItemName { get; set; } = string.Empty;

        private double _value;
        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value.ToLegalDouble();
            }
        }

        public DateTime Time { get; set; }

        public MeasurementTimeValue()
        {

        }
        public MeasurementTimeValue(Measurement measurement)
        {
            Value = measurement.Value;
            Time = measurement.Time;
            ItemEnum = Enum.Parse<ItemEnum>(measurement.Item.Name);
            var item = StateService.Instance.GetSensor(measurement.Item.ItemEnum);
            if (item != null)
            {
                Item = item;    
            }
            else
            {
                Log.Error($"Create MeasurementDto; Item {measurement.Item.Name} doesn't exits");
            }
        }


        public override string ToString()
        {
            return $"{ItemName} {Time.ToShortTimeString()}: {Value}";
        }
    }
}
