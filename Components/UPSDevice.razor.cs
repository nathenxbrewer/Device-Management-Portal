using ManagedPower.Models.API;
using ManagedPower.SDK.Models.API;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ManagedPower.Portal.Components
{
    public partial class UPSDevice
    {

        MudForm form;

        [Parameter]
        public DeviceModel ThisDevice { get; set; }
        public UPSHeartbeat ThisHeartbeat { get; set; } = new();
        public Dictionary<string,object> HeartbeatSummary { get; set; } = new();

        string[] headings = { "Name", "Email", "Gender", "IP Address" };
        private IEnumerable<RebootHistory> RebootHistories = new List<RebootHistory>();

        public List<Alarm> ActiveAlarms { get; set; } = new();
        private string alarmSearchString = "";



        protected override async Task OnInitializedAsync()
        {
            PopulateHeartbeat();
            RebootHistories = await httpClient.GetFromJsonAsync<List<RebootHistory>>($"http://cluw12q05.macsstores.com/managedpower/history/{ThisDevice.devicename}");
            ActiveAlarms = await httpClient.GetFromJsonAsync<List<Alarm>>($"http://cluw12q05.macsstores.com/managedpower/alarms/device/{ThisDevice.id}/open");

        }

        async Task PopulateHeartbeat()
        {
            HeartbeatSummary = new();
            ThisHeartbeat = httpClient.GetFromJsonAsync<List<UPSHeartbeat>>($"http://cluw12q05.macsstores.com/managedpower/ups/heartbeat/device/{ThisDevice.id}/").Result.FirstOrDefault();
            if (ThisHeartbeat != null)
            {
                HeartbeatSummary.Add("Last Heartbeat:", ThisHeartbeat.timestamp);
                HeartbeatSummary.Add("Battery Replacement Date:", ThisHeartbeat.replacementdate);

                HeartbeatSummary.Add("Device Name:", ThisHeartbeat.devicename);
                HeartbeatSummary.Add("Device Mode:", ThisHeartbeat.devicemode);
                HeartbeatSummary.Add("Battery Status:", ThisHeartbeat.batterystatus);
                HeartbeatSummary.Add("Capacity:", ThisHeartbeat.capacity + "%");
                HeartbeatSummary.Add("Input Voltage:", ThisHeartbeat.inputvoltage);
                HeartbeatSummary.Add("Runtime Remaining:", ThisHeartbeat.runtimeremaining + " mins");
                HeartbeatSummary.Add("Time on Battery:", ThisHeartbeat.secondsonbattery + " secs");
                HeartbeatSummary.Add("Temperature:", ThisHeartbeat.temperature + "°");
            }

        }

        private bool FilterAlarmFunc1(Alarm element) => FilterAlarmFunc(element, alarmSearchString);

        private bool FilterAlarmFunc(Alarm element, string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;
            if (element.error.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (element.message.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }
    }
}
