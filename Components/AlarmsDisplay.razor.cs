using ManagedPower.SDK.Models.API;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ManagedPower.Portal.Components
{
    partial class AlarmsDisplay
    {

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Parameter]
        public List<Alarm> Alarms { get; set; }


        void NavigateAlarm(Alarm alarm)
        {
            NavigationManager.NavigateTo($"manage/alarms/alarm?id={alarm.id}");

        }
    }
}
