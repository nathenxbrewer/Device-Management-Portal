using System.Net.Http.Json;
using System.Text.RegularExpressions;
using ManagedPower.Portal.Shared.Dialogs;
using ManagedPower.SDK.Models.API;
using System.Text;
using Newtonsoft.Json;
using CircleK.Mongoose.SDK;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ManagedPower.Portal.Shared;
using System;
using CircleK.Smartsheet;
using ManagedPower.Portal.Models;

namespace ManagedPower.Portal.Pages
{
    public partial class Dashboard
    {



        [Inject]
        public ISnackbar Snackbar { get; set; }

        public List<Alarm> OpenAlarms = new List<Alarm>();
        public List<DeviceModel> AllDevices = new List<DeviceModel>();
        public double HealthPercent = 0.0;


        private HttpClient _httpClient;

        protected override async Task OnInitializedAsync()
        {

            using (var httpClient = new HttpClient())
            {
                OpenAlarms = httpClient.GetFromJsonAsync<List<Alarm>>("http://cluw12q05.macsstores.com/managedpower/alarms/").Result.Where(x => x.statusId == 1).ToList();
                AllDevices = await httpClient.GetFromJsonAsync<List<ManagedPower.SDK.Models.API.DeviceModel>>("http://cluw12q05.macsstores.com/managedpower/devices/Installed");

                HealthPercent = Math.Round(100 - (((double)OpenAlarms.Count() / (double)AllDevices.Count()) * 100), 2);
            }



        }


    }
}
