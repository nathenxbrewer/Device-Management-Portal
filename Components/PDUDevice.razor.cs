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
    public partial class PDUDevice
    {

        MudForm form;

        [Parameter]
        public DeviceModel ThisDevice { get; set; }
        public PDUHeartbeat ThisHeartbeat { get; set; } = new();
        public Dictionary<string,object> HeartbeatSummary { get; set; } = new();

        private IEnumerable<RebootHistory> RebootHistories = new List<RebootHistory>();
        public DeviceModel tempDeviceModel { get; set; } = new();

        public List<Alarm> ActiveAlarms { get; set; } = new();

        private string alarmSearchString = "";


        protected override async Task OnInitializedAsync()
        {
            if (ThisHeartbeat.devicename == null)
            {

                PopulateHeartbeat();
                RebootHistories = await httpClient.GetFromJsonAsync<List<RebootHistory>>($"http://cluw12q05.macsstores.com/managedpower/history/{ThisDevice.devicename}");
                tempDeviceModel = ThisDevice;
                ActiveAlarms = await httpClient.GetFromJsonAsync<List<Alarm>>($"http://cluw12q05.macsstores.com/managedpower/alarms/device/{ThisDevice.id}/open");

            }
        }

        async Task PopulateHeartbeat()
        {
            HeartbeatSummary = new();
            ThisHeartbeat = httpClient.GetFromJsonAsync<List<PDUHeartbeat>>($"http://cluw12q05.macsstores.com/managedpower/pdu/heartbeat/device/{ThisDevice.id}/").Result.FirstOrDefault();
            if (ThisHeartbeat != null)
            {
                HeartbeatSummary.Add("Last Heartbeat:", ThisHeartbeat.timestamp);
                HeartbeatSummary.Add("Device Name:", ThisHeartbeat.devicename);
                HeartbeatSummary.Add("Firmware:", ThisHeartbeat.firmware);
                HeartbeatSummary.Add("IP Address:", ThisHeartbeat.deviceIP);
            }

        }
        private void OnValueChanged()
        {
           // snackbar.Add("Test", Severity.Success);
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
