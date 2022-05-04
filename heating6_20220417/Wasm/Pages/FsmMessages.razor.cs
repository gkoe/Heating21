using Core.Entities;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

using Radzen;
using Radzen.Blazor;

using System;
using System.Linq;
using System.Threading.Tasks;

using Wasm.Services.Contracts;


namespace Wasm.Pages
{
    public partial class FsmMessages
    {
        [Inject]
        public IApiService ApiService { get; set; }
        [Inject]
        public NotificationService NotificationService { get; set; }
        [Inject]
        public IConfiguration Configuration { get; set; }

        public string[] Sensors { get; private set; }

        public DateTime SelectedDate { get; set; } = DateTime.Today;


        public static string[] Fsms => new string[] { "All", "HeatingCircuit", "HotWater", "OilBurner" };

        public string SelectedFsm { get; set; } = "All";

        public FsmTransition[] FsmTransitions { get; set; } = Array.Empty<FsmTransition>();

        public FsmTransition[] SelectedFsmTransitions { get; set; } = Array.Empty<FsmTransition>();

        public RadzenGrid<FsmTransition> FsmTransitionsGrid { get; set; }




        protected override async Task OnInitializedAsync()
        {
            FsmTransitions = await ApiService.GetFsmTransitionsAsync(SelectedDate.Date);
            SetSelectedTransitionsToGrid();
        }

        private void SetSelectedTransitionsToGrid()
        {
            if (SelectedFsm == "All")
            {
                SelectedFsmTransitions = FsmTransitions;
            }
            else
            {
                SelectedFsmTransitions = FsmTransitions.Where(t => t.Fsm == SelectedFsm).ToArray();
            }

        }

        protected void OnChangeFsm(string fsm)
        {
            SelectedFsm = fsm;
            SetSelectedTransitionsToGrid();
        }

        protected async Task OnChangeDate(string date)
        {
            SelectedDate = DateTime.Parse(date);
            FsmTransitions = await ApiService.GetFsmTransitionsAsync(SelectedDate.Date);
            SetSelectedTransitionsToGrid();
        }

    }
}

