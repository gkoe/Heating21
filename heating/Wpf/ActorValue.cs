using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Wpf
{
    public class ActorValue : INotifyPropertyChanged
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
            set 
            { 
                _value = value;
                BoolValue = value != 0.0;
                OnPropertyChanged(); 
            }
        }

        private bool _boolValue;

        public bool BoolValue
        {
            get { return _boolValue; }
            set { _boolValue = value; OnPropertyChanged(); }
        }

        private DateTime _time;
        public DateTime Time
        {
            get { return _time; }
            set { _time = value; OnPropertyChanged(); }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }

    }
}
