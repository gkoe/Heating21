﻿using Core.DataTransferObjects;
using Core.Entities;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

using Radzen;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Wasm.DataTransferObjects;
using Wasm.Services.Contracts;

namespace Wasm.Pages
{
    public partial class ChartSensorValues
    {
        [Inject]
        public IApiService ApiService { get; set; }
        [Inject]
        public NotificationService NotificationService { get; set; }
        [Inject]
        public IConfiguration Configuration { get; set; }

        public string[] Sensors { get; private set; }

        public DateTime SelectedDate { get; set; } = DateTime.Today;

        public string SelectedSensor { get; set; }

        public ChartDataItem[] DataItems { get; set; } = Array.Empty<ChartDataItem>();


        protected override async Task OnInitializedAsync()
        {
            Sensors = Enum.GetNames(typeof(ItemEnum));
            //DataItems = new ChartDataItem[]
            //{
            //    new ChartDataItem { QuarterOfAnHourNumber = 1, Value = 10},
            //    new ChartDataItem { QuarterOfAnHourNumber = 2, Value = 20},
            //    new ChartDataItem { QuarterOfAnHourNumber = 3, Value = 40},
            //    new ChartDataItem { QuarterOfAnHourNumber = 4, Value = 30},
            //    new ChartDataItem { QuarterOfAnHourNumber = 5, Value = 10},
            //    new ChartDataItem { QuarterOfAnHourNumber = 6, Value = 25},
            //    new ChartDataItem { QuarterOfAnHourNumber = 7, Value = 10},
            //    //new ChartDataItem { QuarterOfAnHourNumber = 8, Value = null},
            //    //new ChartDataItem { QuarterOfAnHourNumber = 9, Value = null},
            //    //new ChartDataItem { QuarterOfAnHourNumber = 10, Value = null},
            //    //new ChartDataItem { QuarterOfAnHourNumber = 11, Value = null},
            //};
            SelectedSensor = Sensors[0];
            await GetAndFillDataItemsAsync();


        }

        private async Task GetAndFillDataItemsAsync()
        {
            Console.WriteLine($"GetAndFillDataItemsAsync for Sensor {SelectedSensor} and Date: {SelectedDate.ToShortDateString()}");
            var measurements = await ApiService.GetMeasurementsAsync(SelectedSensor, DateTime.Today);
            DataItems = measurements
                .Select(m => new ChartDataItem
                {
                   QuarterOfAnHourNumber = (m.Time.Hour*60+m.Time.Minute)/15,
                   Value = m.Value
                })
                .ToArray();
        }


        protected async Task OnChangeSensor(string sensor)
        {
            SelectedSensor = sensor;
            await GetAndFillDataItemsAsync();
        }

        protected async Task OnChangeDate(string date)
        {
            SelectedDate = DateTime.Parse(date);
            await GetAndFillDataItemsAsync();
        }

    }
}
