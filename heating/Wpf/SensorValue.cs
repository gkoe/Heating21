using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Wpf
{
    public class SensorValue : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _itemName;
        public string ItemName
        {
            get { return _itemName; }
            set { _itemName = value; OnPropertyChanged(); }
        }

        private double _value = -9999;
        public double Value
        {
            get { return _value; }
            set { _value = value; OnPropertyChanged(); }
        }

        public string ArrowText => GetTrendIconText();
        public string ArrowColor => _trend > 0 ? "Red" : "Blue";
        //{
        //    get { return _arrowText; }
        //}

        private double _trend;
        public double Trend
        {
            get { return _trend; }
            set { _trend = value; OnPropertyChanged(); }
        }

        private DateTime _time;
        public DateTime Time
        {
            get { return _time; }
            set { _time = value; OnPropertyChanged(); }
        }

        private string GetTrendIconText()
        {
            if (Trend > 0.2)
            {
                return "⬆️";
            }
            if (Trend < -0.2)
            {
                return "⬇️";
            }
            return " ";
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }

    }
}
