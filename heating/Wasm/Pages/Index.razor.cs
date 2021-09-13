using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Core.DataTransferObjects;
using Microsoft.AspNetCore.Components;
using Wasm.Services.Contracts;
using Radzen;
using Wasm.Helper;
using Wasm.DataTransferObjects;
using Microsoft.Extensions.Configuration;

namespace Wasm.Pages
{
    public partial class Index : IAsyncDisposable
    {
        [Inject]
        public IApiService ApiService { get; set; }
        [Inject]
        public NotificationService NotificationService { get; set; }
        [Inject]
        public IConfiguration Configuration { get; set; }


        //static readonly string[] list = new string[] { "oi-account-login", "oi-account-logout", "oi-action-redo", "oi-action-undo", "oi-align-center", "oi-align-left", "oi-align-right", "oi-aperture", "oi-arrow-bottom", "oi-arrow-circle-bottom", "oi-arrow-circle-left", "oi-arrow-circle-right", "oi-arrow-circle-top", "oi-arrow-left", "oi-arrow-right", "oi-arrow-thick-bottom", "oi-arrow-thick-left", "oi-arrow-thick-right", "oi-arrow-thick-top", "oi-arrow-top", "oi-audio-spectrum", "oi-audio", "oi-badge", "oi-ban", "oi-bar-chart", "oi-basket", "oi-battery-empty", "oi-battery-full", "oi-beaker", "oi-bell", "oi-bluetooth", "oi-bold", "oi-bolt", "oi-book", "oi-bookmark", "oi-box", "oi-briefcase", "oi-british-pound", "oi-browser", "oi-brush", "oi-bug", "oi-bullhorn", "oi-calculator", "oi-calendar", "oi-camera-slr", "oi-caret-bottom", "oi-caret-left", "oi-caret-right", "oi-caret-top", "oi-cart", "oi-chat", "oi-check", "oi-chevron-bottom", "oi-chevron-left", "oi-chevron-right", "oi-chevron-top", "oi-circle-check", "oi-circle-x", "oi-clipboard", "oi-clock", "oi-cloud-download", "oi-cloud-upload", "oi-cloud", "oi-cloudy", "oi-code", "oi-cog", "oi-collapse-down", "oi-collapse-left", "oi-collapse-right", "oi-collapse-up", "oi-command", "oi-comment-square", "oi-compass", "oi-contrast", "oi-copywriting", "oi-credit-card", "oi-crop", "oi-dashboard", "oi-data-transfer-download", "oi-data-transfer-upload", "oi-delete", "oi-dial", "oi-document", "oi-dollar", "oi-double-quote-sans-left", "oi-double-quote-sans-right", "oi-double-quote-serif-left", "oi-double-quote-serif-right", "oi-droplet", "oi-eject", "oi-elevator", "oi-ellipses", "oi-envelope-closed", "oi-envelope-open", "oi-euro", "oi-excerpt", "oi-expand-down", "oi-expand-left", "oi-expand-right", "oi-expand-up", "oi-external-link", "oi-eye", "oi-eyedropper", "oi-file", "oi-fire", "oi-flag", "oi-flash", "oi-folder", "oi-fork", "oi-fullscreen-enter", "oi-fullscreen-exit", "oi-globe", "oi-graph", "oi-grid-four-up", "oi-grid-three-up", "oi-grid-two-up", "oi-hard-drive", "oi-header", "oi-headphones", "oi-heart", "oi-home", "oi-image", "oi-inbox", "oi-infinity", "oi-info", "oi-italic", "oi-justify-center", "oi-justify-left", "oi-justify-right", "oi-key", "oi-laptop", "oi-layers", "oi-lightbulb", "oi-link-broken", "oi-link-intact", "oi-list-rich", "oi-list", "oi-location", "oi-lock-locked", "oi-lock-unlocked", "oi-loop-circular", "oi-loop-square", "oi-loop", "oi-magnifying-glass", "oi-map-marker", "oi-map", "oi-media-pause", "oi-media-play", "oi-media-record", "oi-media-skip-backward", "oi-media-skip-forward", "oi-media-step-backward", "oi-media-step-forward", "oi-media-stop", "oi-medical-cross", "oi-menu", "oi-microphone", "oi-minus", "oi-monitor", "oi-moon", "oi-move", "oi-musical-note", "oi-paperclip", "oi-pencil", "oi-people", "oi-person", "oi-phone", "oi-pie-chart", "oi-pin", "oi-play-circle", "oi-plus", "oi-power-standby", "oi-print", "oi-project", "oi-pulse", "oi-puzzle-piece", "oi-question-mark", "oi-rain", "oi-random", "oi-reload", "oi-resize-both", "oi-resize-height", "oi-resize-width", "oi-rss-alt", "oi-rss", "oi-script", "oi-share-boxed", "oi-share", "oi-shield", "oi-signal", "oi-signpost", "oi-sort-ascending", "oi-sort-descending", "oi-spreadsheet", "oi-star", "oi-sun", "oi-tablet", "oi-tag", "oi-tags", "oi-target", "oi-task", "oi-terminal", "oi-text", "oi-thumb-down", "oi-thumb-up", "oi-timer", "oi-transfer", "oi-trash", "oi-underline", "oi-vertical-align-bottom", "oi-vertical-align-center", "oi-vertical-align-top", "oi-video", "oi-volume-high", "oi-volume-low", "oi-volume-off", "oi-warning", "oi-wifi", "oi-wrench", "oi-x", "oi-yen", "oi-zoom-in", "oi-zoom-out" };
        private HubConnection hubConnection;
        //private readonly List<string> messages = new List<string>();
        //private string userInput;
        //private string messageInput;

        private readonly Dictionary<string, ActorUiDto> actors = new Dictionary<string, ActorUiDto>();
        private readonly Dictionary<string, SensorWithLastValueUiDto> sensors = new Dictionary<string, SensorWithLastValueUiDto>();

        public bool InManualMode { get; set; }

        // OilBurner
        public SensorWithLastValueUiDto OilBurnerTemperature { get; set; } = new SensorWithLastValueUiDto(nameof(OilBurnerTemperature));
        public ActorUiDto OilBurnerSwitch { get; set; } = new ActorUiDto(nameof(OilBurnerSwitch));
        public string OilBurnerFsmInfo { get; set; } = "";
        public string HeatingCircuitFsmInfo { get; set; } = "";
        public string HotWaterFsmInfo { get; set; } = "";


        // Warmwasser
        public SensorWithLastValueUiDto BoilerTop { get; set; } = new SensorWithLastValueUiDto(nameof(BoilerTop));
        public SensorWithLastValueUiDto BoilerBottom { get; set; } = new SensorWithLastValueUiDto(nameof(BoilerBottom));
        public SensorWithLastValueUiDto SolarCollector { get; set; } = new SensorWithLastValueUiDto(nameof(SolarCollector));
        public SensorWithLastValueUiDto BufferTop { get; set; } = new SensorWithLastValueUiDto(nameof(BufferTop));
        public SensorWithLastValueUiDto BufferBottom { get; set; } = new SensorWithLastValueUiDto(nameof(BufferBottom));

        public ActorUiDto PumpBoiler { get; set; } = new ActorUiDto(nameof(PumpBoiler));
        public ActorUiDto PumpSolar { get; set; } = new ActorUiDto(nameof(PumpSolar));
        public ActorUiDto ValveBoilerBuffer { get; set; } = new ActorUiDto(nameof(ValveBoilerBuffer));

        // Heizung
        public SensorWithLastValueUiDto LivingroomFirstFloor { get; set; } = new SensorWithLastValueUiDto(nameof(LivingroomFirstFloor));
        public SensorWithLastValueUiDto LivingroomGroundFloor { get; set; } = new SensorWithLastValueUiDto(nameof(LivingroomGroundFloor));
        public SensorWithLastValueUiDto TemperatureBefore { get; set; } = new SensorWithLastValueUiDto(nameof(TemperatureBefore));
        public SensorWithLastValueUiDto TemperatureAfter { get; set; } = new SensorWithLastValueUiDto(nameof(TemperatureAfter));
        public SensorWithLastValueUiDto TemperatureFirstFloor { get; set; } = new SensorWithLastValueUiDto(nameof(TemperatureFirstFloor));
        public SensorWithLastValueUiDto TemperatureGroundFloor { get; set; } = new SensorWithLastValueUiDto(nameof(TemperatureGroundFloor));

        public ActorUiDto PumpFirstFloor { get; set; } = new ActorUiDto(nameof(PumpFirstFloor));
        public ActorUiDto MixerFirstFloorPlus { get; set; } = new ActorUiDto(nameof(MixerFirstFloorPlus));
        public ActorUiDto MixerFirstFloorMinus { get; set; } = new ActorUiDto(nameof(MixerFirstFloorMinus));
        public ActorUiDto PumpGroundFloor { get; set; } = new ActorUiDto(nameof(PumpGroundFloor));
        public ActorUiDto MixerGroundFloorPlus { get; set; } = new ActorUiDto(nameof(MixerGroundFloorPlus));
        public ActorUiDto MixerGroundFloorMinus { get; set; } = new ActorUiDto(nameof(MixerGroundFloorMinus));

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine($"------------------- SignalRHubUrl: {Configuration["SignalRHubUrl"]}");
            hubConnection = new HubConnectionBuilder()
                .WithUrl(Configuration["SignalRHubUrl"])
                //.WithUrl("https://localhost:5001/measurementshub")
                //.WithUrl("https://heatingapi.dynv6.net/measurementshub")
                .Build();

            //hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            //{
            //    //var encodedMsg = $"{user}: {message}";
            //    //messages.Add(encodedMsg);
            //    //StateHasChanged();
            //});

            actors.Add(nameof(OilBurnerSwitch), OilBurnerSwitch);
            actors.Add(nameof(PumpBoiler), PumpBoiler);
            actors.Add(nameof(PumpSolar), PumpSolar);
            actors.Add(nameof(PumpFirstFloor), PumpFirstFloor);
            actors.Add(nameof(PumpGroundFloor), PumpGroundFloor);
            actors.Add(nameof(ValveBoilerBuffer), ValveBoilerBuffer);
            actors.Add(nameof(MixerFirstFloorMinus), MixerFirstFloorMinus);
            actors.Add(nameof(MixerFirstFloorPlus), MixerFirstFloorPlus);
            actors.Add(nameof(MixerGroundFloorMinus), MixerGroundFloorMinus);
            actors.Add(nameof(MixerGroundFloorPlus), MixerGroundFloorPlus);

            foreach (var item in actors.Values)
            {
                item.ApiService = ApiService;
            }

            sensors.Add(nameof(OilBurnerTemperature), OilBurnerTemperature);
            sensors.Add(nameof(BoilerTop), BoilerTop);
            sensors.Add(nameof(BoilerBottom), BoilerBottom);
            sensors.Add(nameof(BufferTop), BufferTop);
            sensors.Add(nameof(BufferBottom), BufferBottom);
            sensors.Add(nameof(LivingroomFirstFloor), LivingroomFirstFloor);
            sensors.Add(nameof(LivingroomGroundFloor), LivingroomGroundFloor);
            sensors.Add(nameof(TemperatureAfter), TemperatureAfter);
            sensors.Add(nameof(TemperatureBefore), TemperatureBefore);
            sensors.Add(nameof(TemperatureFirstFloor), TemperatureFirstFloor);
            sensors.Add(nameof(TemperatureGroundFloor), TemperatureGroundFloor);
            sensors.Add(nameof(SolarCollector), SolarCollector);

            hubConnection.On<MeasurementDto>("ReceiveMeasurement", (measurement) =>
            {
                var trendPercent = 0.0;
                if (measurement.Value != 0)
                {
                    trendPercent = measurement.Trend / measurement.Value * 100;
                }

                var encodedMsg = $"{measurement.SensorName},  {measurement.Time}, {measurement.Value}, {trendPercent:F2}%";
                //            var encodedMsg = $"{measurement.SensorName},  {measurement.Time}, {measurement.Value}, {measurement.Trend}";
                //messages.Add(encodedMsg);
                Console.WriteLine(encodedMsg);
                if (sensors.ContainsKey(measurement.SensorName))
                {
                    var sensor = sensors[measurement.SensorName];
                    sensor.Value = measurement.Value;
                    sensor.Trend = trendPercent;
                }
                else if (actors.ContainsKey(measurement.SensorName))
                {
                    var actor = actors[measurement.SensorName];
                    actor.NewActorValueReceived(measurement);
                }
                else
                {
                    throw new Exception($"{measurement.SensorName} is not a known sensor or actor");
                }
                StateHasChanged();
            });

            hubConnection.On<FsmStateChangedInfoDto>("ReceiveFsmStateChanged", (fsmInfo) =>
            {
                if (fsmInfo.Fsm == "OilBurner")
                {
                    OilBurnerFsmInfo = $"{fsmInfo.LastState} > {fsmInfo.Input} > {fsmInfo.ActState}";
                    Console.WriteLine(OilBurnerFsmInfo);
                }
                if (fsmInfo.Fsm == "HeatingCircuit")
                {
                    HeatingCircuitFsmInfo = $"{fsmInfo.LastState} > {fsmInfo.Input} > {fsmInfo.ActState}";
                    Console.WriteLine(HeatingCircuitFsmInfo);
                }
                if (fsmInfo.Fsm == "HotWater")
                {
                    HotWaterFsmInfo = $"{fsmInfo.LastState} > {fsmInfo.Input} > {fsmInfo.ActState}";
                    Console.WriteLine(HotWaterFsmInfo);
                }
                StateHasChanged();
            });

            await hubConnection.StartAsync();
        }


        protected async Task MixerChangedAysnc(string mixerName)
        {
            if (!actors.ContainsKey(mixerName))
            {
                throw new ArgumentException($"Actor: {mixerName} existiert nicht");
            }
            var mixer = actors[mixerName];
            string otherMixerName = mixerName switch
            {
                nameof(MixerFirstFloorMinus) => nameof(MixerFirstFloorPlus),
                nameof(MixerFirstFloorPlus) => nameof(MixerFirstFloorMinus),
                nameof(MixerGroundFloorMinus) => nameof(MixerGroundFloorPlus),
                nameof(MixerGroundFloorPlus) => nameof(MixerGroundFloorMinus),
                _ => throw new NotImplementedException()
            };
            var otherMixer = actors[otherMixerName];
            if (otherMixer.IsOn)
            {
                otherMixer.IsOn = false;
                Console.WriteLine($"OtherMixer {otherMixer.Name} abschalten");
                await otherMixer.SwitchActorPerApiAsync();
            }
            await mixer.SwitchActorPerApiAsync();
        }

        protected async Task SwitchChangedAsync(string actorName)
        {
            if (!actors.ContainsKey(actorName))
            {
                throw new ArgumentException($"Actor: {actorName} existiert nicht");
            }
            var actor = actors[actorName];
            await actor.SwitchActorPerApiAsync();
        }

        protected void IsManualModeChanged()
        {
            foreach (var actor in actors.Values)
            {
                actor.InManualMode = InManualMode;
            }
        }

        //async Task Send() =>
        //    await hubConnection.SendAsync("SendMessage", userInput, messageInput);

        public bool IsConnected =>
            hubConnection.State == HubConnectionState.Connected;

        public async Task ResetEsp()
        {
            Console.WriteLine("Reset Esp");
            await ApiService.ResetEspAsync();
            NotificationService.ShowNotification(NotificationSeverity.Info, "ESP reseted");
        }



        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                Console.WriteLine("SignalR-Connection disposed!");
                await hubConnection.DisposeAsync();
            }
        }

    }
}

