using ManagedPower.Models.API;
using ManagedPower.Portal.Models;
using ManagedPower.SDK.Models.API;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ManagedPower.Portal.Pages.Manage
{
    public partial class Devices
    {
        [Inject]
        public IDialogService DialogService { get; set; }
        MudMessageBox mbox { get; set; }


        async Task DeleteSelected()
        {
            
            bool? result = await mbox.Show();

            if (result == true)
            {
                foreach (var device in selectedItems)
                {
                    DeleteDevice(device);
                }
                //Snackbar.Add($"Success", Severity.Success);
            }
            else
            {
                Snackbar.Add($"User cancelled", Severity.Error);
            }


        }


    }
}
