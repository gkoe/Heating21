using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Core.DataTransferObjects;
using Microsoft.AspNetCore.Components;
using Wasm.Services.Contracts;

namespace Wasm.Pages
{
    public partial class Index : IAsyncDisposable
    {
        [Inject]
        public IApiService ApiService { get; set; }

        static readonly string[] list = new string[] { "oi-account-login", "oi-account-logout", "oi-action-redo", "oi-action-undo", "oi-align-center", "oi-align-left", "oi-align-right", "oi-aperture", "oi-arrow-bottom", "oi-arrow-circle-bottom", "oi-arrow-circle-left", "oi-arrow-circle-right", "oi-arrow-circle-top", "oi-arrow-left", "oi-arrow-right", "oi-arrow-thick-bottom", "oi-arrow-thick-left", "oi-arrow-thick-right", "oi-arrow-thick-top", "oi-arrow-top", "oi-audio-spectrum", "oi-audio", "oi-badge", "oi-ban", "oi-bar-chart", "oi-basket", "oi-battery-empty", "oi-battery-full", "oi-beaker", "oi-bell", "oi-bluetooth", "oi-bold", "oi-bolt", "oi-book", "oi-bookmark", "oi-box", "oi-briefcase", "oi-british-pound", "oi-browser", "oi-brush", "oi-bug", "oi-bullhorn", "oi-calculator", "oi-calendar", "oi-camera-slr", "oi-caret-bottom", "oi-caret-left", "oi-caret-right", "oi-caret-top", "oi-cart", "oi-chat", "oi-check", "oi-chevron-bottom", "oi-chevron-left", "oi-chevron-right", "oi-chevron-top", "oi-circle-check", "oi-circle-x", "oi-clipboard", "oi-clock", "oi-cloud-download", "oi-cloud-upload", "oi-cloud", "oi-cloudy", "oi-code", "oi-cog", "oi-collapse-down", "oi-collapse-left", "oi-collapse-right", "oi-collapse-up", "oi-command", "oi-comment-square", "oi-compass", "oi-contrast", "oi-copywriting", "oi-credit-card", "oi-crop", "oi-dashboard", "oi-data-transfer-download", "oi-data-transfer-upload", "oi-delete", "oi-dial", "oi-document", "oi-dollar", "oi-double-quote-sans-left", "oi-double-quote-sans-right", "oi-double-quote-serif-left", "oi-double-quote-serif-right", "oi-droplet", "oi-eject", "oi-elevator", "oi-ellipses", "oi-envelope-closed", "oi-envelope-open", "oi-euro", "oi-excerpt", "oi-expand-down", "oi-expand-left", "oi-expand-right", "oi-expand-up", "oi-external-link", "oi-eye", "oi-eyedropper", "oi-file", "oi-fire", "oi-flag", "oi-flash", "oi-folder", "oi-fork", "oi-fullscreen-enter", "oi-fullscreen-exit", "oi-globe", "oi-graph", "oi-grid-four-up", "oi-grid-three-up", "oi-grid-two-up", "oi-hard-drive", "oi-header", "oi-headphones", "oi-heart", "oi-home", "oi-image", "oi-inbox", "oi-infinity", "oi-info", "oi-italic", "oi-justify-center", "oi-justify-left", "oi-justify-right", "oi-key", "oi-laptop", "oi-layers", "oi-lightbulb", "oi-link-broken", "oi-link-intact", "oi-list-rich", "oi-list", "oi-location", "oi-lock-locked", "oi-lock-unlocked", "oi-loop-circular", "oi-loop-square", "oi-loop", "oi-magnifying-glass", "oi-map-marker", "oi-map", "oi-media-pause", "oi-media-play", "oi-media-record", "oi-media-skip-backward", "oi-media-skip-forward", "oi-media-step-backward", "oi-media-step-forward", "oi-media-stop", "oi-medical-cross", "oi-menu", "oi-microphone", "oi-minus", "oi-monitor", "oi-moon", "oi-move", "oi-musical-note", "oi-paperclip", "oi-pencil", "oi-people", "oi-person", "oi-phone", "oi-pie-chart", "oi-pin", "oi-play-circle", "oi-plus", "oi-power-standby", "oi-print", "oi-project", "oi-pulse", "oi-puzzle-piece", "oi-question-mark", "oi-rain", "oi-random", "oi-reload", "oi-resize-both", "oi-resize-height", "oi-resize-width", "oi-rss-alt", "oi-rss", "oi-script", "oi-share-boxed", "oi-share", "oi-shield", "oi-signal", "oi-signpost", "oi-sort-ascending", "oi-sort-descending", "oi-spreadsheet", "oi-star", "oi-sun", "oi-tablet", "oi-tag", "oi-tags", "oi-target", "oi-task", "oi-terminal", "oi-text", "oi-thumb-down", "oi-thumb-up", "oi-timer", "oi-transfer", "oi-trash", "oi-underline", "oi-vertical-align-bottom", "oi-vertical-align-center", "oi-vertical-align-top", "oi-video", "oi-volume-high", "oi-volume-low", "oi-volume-off", "oi-warning", "oi-wifi", "oi-wrench", "oi-x", "oi-yen", "oi-zoom-in", "oi-zoom-out" };
        private HubConnection hubConnection;
        private List<string> messages = new List<string>();
        private string userInput;
        private string messageInput;

        public double NtcValue { get; set; }
        public double NtcTrend { get; set; }
        public string NtcTrendIcon { get; set; }

        public double OneWireValue { get; set; }
        public double OneWireTrend { get; set; }
        public string OneWireTrendIcon { get; set; }

        public double HttpValue { get; set; }
        public double HttpTrend { get; set; }
        public string HttpTrendIcon { get; set; }

        bool _switchIsOn = false;
        public bool SwitchIsOn
        {
            get
            {
                return _switchIsOn;
            }
            set
            {
                ButtonSwitchDisabled = true;
                _switchIsOn = value;
                //if (_switchIsOn == null)
                //{
                //    Console.WriteLine("_switchIsOn is null");
                //    return;
                //}
                if (_switchIsOn)
                {
                    ApiService.ChangeSwitchAsync("ssr00", true);
                }
                else
                {
                    ApiService.ChangeSwitchAsync("ssr00", false);
                }

            }
        }

        public bool ButtonSwitchDisabled { get; set; }

        protected override async Task OnInitializedAsync()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/measurementshub")
                //.WithUrl("https://heatingapi.dynv6.net/measurementshub")
                .Build();

            //hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            //{
            //    //var encodedMsg = $"{user}: {message}";
            //    //messages.Add(encodedMsg);
            //    //StateHasChanged();
            //});

            hubConnection.On<MeasurementDto>("ReceiveMeasurement", (measurement) =>
            {
                var trendPercent = 0.0;
                if (measurement.Value != 0)
                {
                    trendPercent = measurement.Trend / measurement.Value * 100;
                }
                
                var encodedMsg = $"{measurement.SensorName},  {measurement.Time}, {measurement.Value}, {trendPercent:F2}%";
                //            var encodedMsg = $"{measurement.SensorName},  {measurement.Time}, {measurement.Value}, {measurement.Trend}";
                messages.Add(encodedMsg);
                Console.WriteLine(encodedMsg);
                switch (measurement.SensorName)
                {
                    case "temperature":
                        {
                            HttpValue = measurement.Value;
                            HttpTrend = measurement.Trend;
                            HttpTrendIcon = GetTrendIconForTrend(measurement.Trend);
                            break;
                        }
                    case "temperature_ntc":
                        {
                            NtcValue = measurement.Value;
                            NtcTrend = measurement.Trend;
                            NtcTrendIcon = GetTrendIconForTrend(measurement.Trend);
                            break;
                        }
                    case "temperature_onewire":
                        {
                            OneWireValue = measurement.Value;
                            OneWireTrend = measurement.Trend;
                            OneWireTrendIcon = GetTrendIconForTrend(measurement.Trend);
                            break;
                        }
                    case "ssr00":
                        {
                            ButtonSwitchDisabled = false;
                            break;
                        }

                    default:
                        break;
                }
                StateHasChanged();
            });

            await hubConnection.StartAsync();
        }

        private string GetTrendIconForTrend(double trend)
        {
            if (trend > 0.02)
            {
                return "oi-arrow-circle-top";
            }
            else if (trend < -0.02)
            {
                return "oi-arrow-circle-bottom";
            }
                return "oi-arrow-circle-right";
        }

        //async Task Send() =>
        //    await hubConnection.SendAsync("SendMessage", userInput, messageInput);

        public bool IsConnected =>
            hubConnection.State == HubConnectionState.Connected;

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

