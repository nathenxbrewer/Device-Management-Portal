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
using System.IO;
using System.Drawing;

namespace ManagedPower.Portal.Pages.Manage
{

    public partial class Registration
    {
        [Inject]
        public HttpClient httpClient { get; set; }

        [Inject]
        public ISnackbar Snackbar { get; set; }

        private Device selectedItem = new Device();

        private IEnumerable<Device> Elements = new List<Device>();

        private MudTable<Device> mudTable = new();
        private List<string> clickedEvents = new();
        private int selectedRowNumber = -1;

        MudSelect<string> lanenumberfield;

        private Dictionary<int,string> LaneNumbers = new Dictionary<int,string>();

        private List<DeviceModel> AllDevices = new List<DeviceModel>();
        private List<DeviceModel> TheseDevices = new List<DeviceModel>();

        public bool ManualEntry { get; set; } = false;
        public bool StoreRequired { get; set; } = true;

        private HttpClient _httpClient;

        [Inject]
        public NavigationManager NavigationManager { get; set; }


        protected override async Task OnInitializedAsync()
        {  

            LaneNumbers.Add(0, "Back Rack");
            LaneNumbers.Add(1, "Lane 1");
            LaneNumbers.Add(2, "Lane 2");
            LaneNumbers.Add(3, "Lane 3");
            LaneNumbers.Add(4, "Lane 4");
            LaneNumbers.Add(5, "Lane 5");

            if (!string.IsNullOrEmpty(storeNum))
            {
                StoreNumberField.Value = storeNum;
                StoreRequired = false;
                StoreNumberField.Disabled = true;
                await OnStoreNumChanged();
            }

            AllLocations = await httpClient.GetFromJsonAsync<List<ManagedPower.SDK.Models.API.Location>>("http://cluw12q05.macsstores.com/managedpower/locations/");
            AllDevices = await httpClient.GetFromJsonAsync<List<DeviceModel>>("http://cluw12q05.macsstores.com/managedpower/devices/Installed");

        }


        private async Task OnStoreNumChanged()
        {
            if (StoreNumberField.Value != null)
            {
                if (StoreNumberField.Text.Length == 7)
                {


                TheseDevices = httpClient.GetFromJsonAsync<List<DeviceModel>>("http://cluw12q05.macsstores.com/managedpower/devices/Installed").Result
                    .Where(x=>x.store == StoreNumberField.Text).ToList();

                //var existinglanenumbers = TheseDevices.Select(x => x.lanenumber);
                //foreach (var lane in existinglanenumbers)
                //{
                //    LaneNumbers.Remove(LaneNumbers.FirstOrDefault(x=>x.Key == lane).Key);

                //}

                //deviceNumbers = LaneNumbers.Select(x=>x.Value).ToArray();

                storeNum = StoreNumberField.Value;
                var store = new Store(storeNum);
                if (store.devices.Count > 0)
                {
                    var routerarp = store.devices.FirstOrDefault(x => x.DefaultType == "Router").LastARP.Split('\n').First();

                    var PDUsFound = store.devices.Where(x => x.MAC.StartsWith("00:06:67:") && x.LastARP.Split('\n').Any(y => y == routerarp)).ToList();
                    if (PDUsFound.Count() > 0)
                    {
                        Elements = PDUsFound;
                    }
                    else
                    {
                        bool? result = await DialogService.ShowMessageBox(
    "Warning",
    "The store you have selected does not contain any TrippLite devices. Would you like to enter the device manually?",
    yesText: "Yes", cancelText: "Cancel");
                        if (result == true)
                        {
                                ManualEntry = true;
                        }
                        else
                        {
                            Snackbar.Add($"Device Registration cancelled", Severity.Error);
                                storeNum = "";
                                ClearForm();
                        }
                    }
                }
                    else
                    {
                        bool? result = await DialogService.ShowMessageBox(
    "Error",
    "The store entered is not a valid Store Number!",
    yesText: "My Bad");

                        storeNum = "";
                        ClearForm();
                    }
                }
                StateHasChanged();
            }
        }

        private void OnSelectedItemsChanged()
        {
            //if (selectedItems.Count() > 0)
            //{
            //    selectedItems = selectedItems.Take(2).ToHashSet();

            //}
        }

        private void RowClickEvent(TableRowClickEventArgs<Device> tableRowClickEventArgs)
        {
            clickedEvents.Add("Row has been clicked");
        }

        private string SelectedRowClassFunc(Device element, int rowNumber)
        {
            //if (selectedRowNumber == rowNumber)
            //{
            //    selectedRowNumber = -1;
            //    clickedEvents.Add("Selected Row: None");
            //    return string.Empty;
            //}
            //else
            if (mudTable.SelectedItem != null && mudTable.SelectedItem.Equals(element))
            {
                selectedRowNumber = rowNumber;
                clickedEvents.Add($"Selected Row: {rowNumber}");
                return "selected";
            }
            else
            {
                return string.Empty;
            }
        }
        private async void OnButtonClicked()
        {
            if (selectedItem.IP != null || ManualEntry == true)
            {

            if (storeLive)
            {
                bool? result = await DialogService.ShowMessageBox(
                    "Warning",
                    "Please confirm information added is accurate before registering device",
                    yesText: "Register", cancelText: "Cancel");
                if (result == true)
                {
                    deviceIp = selectedItem.IP;
                    deviceMac = selectedItem.MAC;
                        deviceName = StoreNumberField.Text + deviceType.Substring(0,2) + LaneNumbers.FirstOrDefault(x => x.Value == lanenumberfield.Text).Key;
                    //not grabbing serial number during registration. Serial number is populated on heartbeat.
                    RegisterDevice();
                    ClearForm();
                }
                else
                {
                    Snackbar.Add($"Device Registration cancelled", Severity.Error);
                }
            }
            else
            {
                bool? result = await DialogService.ShowMessageBox(
                    "Warning",
                    "The store you have selected is not currently registered, would you like to register the store?",
                    yesText: "Register Store", cancelText: "Cancel");
                if (result == true)
                {
                    await AddNewLocation();
                }
                else
                {
                    Snackbar.Add($"Store was not created", Severity.Error);
                }
            }

            }
            else
            {
                SelectedRowClassFunc(selectedItem, selectedRowNumber);

                bool? result = await DialogService.ShowMessageBox(
                    "Error",
                    "Please select a device from the table.",
                    yesText: "OK");

            }
        }

        async Task AddNewLocation()
        {
            var selectedsite = Vars.AllSites.FirstOrDefault(x => x.StoreNumber == StoreNumberField.Text);
            var parameters = new DialogParameters { ["SelectedSite"] = selectedsite };

            var dialog = DialogService.Show<NewLocationDialog>("Add Location", parameters);
            var result = await dialog.Result;

            if (!result.Cancelled)
            {
                Snackbar.Add($"Location Added!", Severity.Success);
            }
        }



        private async void RegisterDevice()
        {
            if (!TheseDevices.Select(x=>x.devicename).Contains(deviceName))
            {
            int deviceTypeNum = 0;
            if (deviceType == "PDU")
            {
                deviceTypeNum = 1;
            }

            int lanenumber = Array.IndexOf(deviceNumbers, laneNum);


            //var store = new CircleK.Mongoose.SDK.Store(storeNum);
            //var possibledevice = store.devices.Where(x => x.DefaultType.StartsWith($"{deviceType} {lanenumber}")).ToList();


            var device = new ManagedPower.SDK.Models.API.DeviceModel()
            {
                devicename = deviceName,
                deviceip = deviceIp,
                devicetype = deviceTypeNum,
                store = storeNum,
                lanenumber = lanenumber,
                serial = deviceSerial,
                mac = deviceMac,
                installdate = DateTime.Now,
                activated = true,
                status = "Installed",
                batteryinstalldate = batteryinstalldate,
                modified = DateTime.Now,
            };

                if (deviceType == "PDU")
                {
                    batteryinstalldate = DateTime.MinValue;
                }

            using (var client = new HttpClient())
            {
                var jsonstring = JsonConvert.SerializeObject(device);
                var res = client.PostAsync("http://cluw12q05.macsstores.com/managedpower/devices",
                    new StringContent(jsonstring,
                    Encoding.UTF8, "application/json")
                );
                try
                {
                    var result = res.Result.EnsureSuccessStatusCode();
                    if (result.IsSuccessStatusCode)
                    {
                        Snackbar.Add($"Device {deviceName} has been registered", Severity.Success);
                    }

                }
                catch (Exception e)
                {
                    Snackbar.Add($"Device {deviceName} failed to register", Severity.Error);

                }
            }

            }
            else
            {
                Snackbar.Add($"Device {deviceName} already exists!", Severity.Warning);

            }
        }

        private void ClearForm()
        {
            deviceType = "";
            laneNum = "";
            deviceIp = "";
            deviceName = "";
            deviceSerial = "";
            deviceMac = "";
            form.Reset();
        }

    }
}
