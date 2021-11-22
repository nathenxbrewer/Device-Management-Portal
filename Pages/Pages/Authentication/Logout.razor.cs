using ManagedPower.Portal.Shared;
using System.Threading.Tasks;

namespace ManagedPower.Portal.Pages.Pages.Authentication
{
    public partial class Logout
    {
        protected override async Task OnInitializedAsync()
        {
            
            Vars.LoggedInUser = null;
            Vars.base64UserPhoto = null;
        }
        }
    }
