using MahApps.Metro.Controls;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

namespace Heating.Wpf
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        const string BaseUrl = "http://10.0.0.12/";
        const string BaseUrlSensor = $"{BaseUrl}sensor?";
        const string BaseUrlActor = $"{BaseUrl}actor?";

        public HttpClient HttpClient { get;  }

        public Sensor OilBurnerTemperature { get; }


        public MainWindow()
        {
            InitializeComponent();
            HttpClient = new HttpClient();
            Loaded += MainWindow_Loaded;
            OilBurnerTemperature = new Sensor
            {
                Name = nameof(OilBurnerTemperature)
            };
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => ReadSensorsAndActors());
        }

        public async Task ReadSensorsAndActors()
        {
            await ReadInSensors();
        }

        public async Task ReadInSensors()
        {
            await ReadInSensor(OilBurnerTemperature);
        }


        // {"sensor": OilBurnerTemperature,"time": 2022-01-28 19:25:25,"value": 26.85 Grad}
        public async Task ReadInSensor(Sensor sensor)
        {
            var url = $"{BaseUrlSensor}{sensor.Name}";
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
            int endPos = response.IndexOf(endString, startPos+startString.Length);
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
    }
}
