using Base.ExtensionMethods;

using Core.DataTransferObjects;

using MahApps.Metro.Controls;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        const string BaseUrlEsp = "http://10.0.0.12/";
        readonly string BaseUrlSensorEsp = $"{BaseUrlEsp}sensor?";
        readonly string BaseUrlActorEsp = $"{BaseUrlEsp}actor?";
        private const string BaseUrlApi = "http://10.0.0.1:5000/api";


        public HttpClient HttpClient { get; }
        public event PropertyChangedEventHandler PropertyChanged;

        private string _measurementTime = "00:00";

        public string StatusText
        {
            get { return _measurementTime; }
            set { _measurementTime = value; OnPropertyChanged(); }
        }


        private SensorValue _oilBurnerTemperature = new() { ItemName = nameof(OilBurnerTemperature) };
        public SensorValue OilBurnerTemperature
        {
            get { return _oilBurnerTemperature; }
            set { _oilBurnerTemperature = value; OnPropertyChanged(); }
        }

        private SensorValue _hmoLivingroomFirstFloor = new() { ItemName = nameof(HmoLivingroomFirstFloor) };
        public SensorValue HmoLivingroomFirstFloor
        {
            get { return _hmoLivingroomFirstFloor; }
            set { _hmoLivingroomFirstFloor = value; OnPropertyChanged(); }
        }

        private SensorValue _boilerTop = new() { ItemName = nameof(BoilerTop) };
        public SensorValue BoilerTop
        {
            get { return _boilerTop; }
            set { _boilerTop = value; OnPropertyChanged(); }
        }

        private SensorValue _solarCollector = new() { ItemName = nameof(SolarCollector) };
        public SensorValue SolarCollector
        {
            get { return _solarCollector; }
            set { _solarCollector = value; OnPropertyChanged(); }
        }

        private ActorValue _oilBurnerSwitch = new() { ItemName = nameof(OilBurnerSwitch) };
        public ActorValue OilBurnerSwitch
        {
            get { return _oilBurnerSwitch; }
            set { _oilBurnerSwitch = value; OnPropertyChanged(); }
        }

        private ActorValue _pumpBoiler = new() { ItemName = nameof(PumpBoiler) };
        public ActorValue PumpBoiler
        {
            get { return _pumpBoiler; }
            set { _pumpBoiler = value; OnPropertyChanged(); }
        }

        private ActorValue _pumpSolar = new() { ItemName = nameof(PumpSolar) };
        public ActorValue PumpSolar
        {
            get { return _pumpSolar; }
            set { _pumpSolar = value; OnPropertyChanged(); }
        }

        private ActorValue _pumpFirstFloor = new() { ItemName = nameof(PumpFirstFloor) };
        public ActorValue PumpFirstFloor
        {
            get { return _pumpFirstFloor; }
            set { _pumpFirstFloor = value; OnPropertyChanged(); }
        }









        public ObservableCollection<SensorValue> Sensors { get; set; } = new ObservableCollection<SensorValue>();
        public ObservableCollection<ActorValue> Actors { get; set; } = new ObservableCollection<ActorValue>();

        public MainWindow()
        {
            Sensors.Add(OilBurnerTemperature);
            Sensors.Add(HmoLivingroomFirstFloor);
            Sensors.Add(BoilerTop);
            Sensors.Add(SolarCollector);

            Actors.Add(OilBurnerSwitch);
            Actors.Add(PumpBoiler);
            Actors.Add(PumpFirstFloor);
            Actors.Add(PumpSolar);

            InitializeComponent();
            HttpClient = new HttpClient();
            Loaded += MainWindow_Loaded;
            DataContext = this;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Task.Run(() => ReadSensorsAndActorsFromEsp());
            //_ = Task.Run(() => ReadSensorsAndActorsFromApi());
            ReadSensorsAndActorsFromApi();
            DispatcherTimer timer = new()
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
            //OilBurnerTemperature.Value = 88;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ReadSensorsAndActorsFromApi();
        }

        private string _toolTipText = "Hello";

        public string ToolTipText
        {
            get { return _toolTipText; }
            set { _toolTipText = value; OnPropertyChanged(); }
        }


        private async void ReadSensorsAndActorsFromApi()
        {
            string url = $"{BaseUrlApi}/ruleengine/getsensorandactorvalues";
            HttpResponseMessage response = await HttpClient.GetAsync(url);
            string responseString = await response.Content.ReadAsStringAsync();
            MeasurementDto[] measurementDtos = JsonConvert.DeserializeObject<MeasurementDto[]>(responseString);
            if (measurementDtos != null)
            {
                foreach (MeasurementDto measurement in measurementDtos)
                {
                    SensorValue sensor = Sensors.SingleOrDefault(i => i.ItemName == measurement.ItemName);
                    if (sensor != null)
                    {
                        sensor.Value = measurement.Value;
                        sensor.Time = measurement.Time;
                        sensor.Trend = measurement.Trend;
                        OnPropertyChanged(sensor.ItemName);
                    }
                    ActorValue actor = Actors.SingleOrDefault(i => i.ItemName == measurement.ItemName);
                    if (actor != null)
                    {
                        actor.Value = measurement.Value;
                        actor.Time = measurement.Time;
                        OnPropertyChanged(actor.ItemName);
                    }
                }
                StatusText = "Werte zuletzt aktualisiert um " + DateTime.Now.ToShortTimeString();
            }
        }

        void NotifyAllProperties()
        {
            OnPropertyChanged(nameof(OilBurnerTemperature));
        }

        public async Task ReadSensorsAndActorsFromEsp()
        {
            await ReadInSensorsFromEsp();
        }

        public async Task ReadInSensorsFromEsp()
        {
            await ReadInSensorFromEsp(OilBurnerTemperature);
        }


        // {"sensor": OilBurnerTemperature,"time": 2022-01-28 19:25:25,"value": 26.85 Grad}
        public async Task ReadInSensorFromEsp(SensorValue sensor)
        {
            var url = $"{BaseUrlSensorEsp}{sensor.ItemName}";
            var response = await HttpClient.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            string timeText = GetFromResponse(responseString, "\"time\": ", ",");
            string valueText = GetFromResponse(responseString, "\"value\": ", " ");
            sensor.Time = DateTime.Parse(timeText);
            sensor.Value = valueText.TryParseToDouble() ?? -999;
        }

        private string GetFromResponse(string response, string startString, string endString)
        {
            int startPos = response.IndexOf(startString);
            int endPos = response.IndexOf(endString, startPos + startString.Length);
            if (startPos == -1 || endPos == -1) return "";
            startPos += startString.Length;
            return response[startPos..endPos];
        }

        static HttpClient GetHttpClient()
        {
            var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            var httpClient = httpClientFactory!.CreateClient();
            return httpClient;

        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new(propertyName));
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            var binding = textBlock.GetBindingExpression(TextBlock.TextProperty);
            var bindingPath = binding.ParentBinding.Path.Path;
            var elements = bindingPath?.Split('.');
            if (elements != null && elements.Length >= 2)
            {
                var sensorName = elements[0];
                var sensor = Sensors.FirstOrDefault(s => s.ItemName == sensorName);
                if (sensor != null)
                {
                    ToolTipText = sensor.Time.ToString("HH:mm");
                }
            }
        }

        private void CheckBox_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            var binding = checkBox.GetBindingExpression(CheckBox.IsCheckedProperty);
            var bindingPath = binding.ParentBinding.Path.Path;
            var elements = bindingPath?.Split('.');
            if (elements != null && elements.Length >= 2)
            {
                var actorName = elements[0];
                var actor = Actors.FirstOrDefault(s => s.ItemName == actorName);
                if (actor != null)
                {
                    ToolTipText = actor.Time.ToString("HH:mm");
                }
            }
        }
    }
}
