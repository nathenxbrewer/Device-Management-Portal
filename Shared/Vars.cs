using CircleK.Smartsheet.Models;
using ManagedPower.Portal.Models;
using System.Collections.Generic;

namespace ManagedPower.Portal.Shared
{
    public static class Vars
    {
        public static List<Site> AllSites { get; set; }

        public static string base64UserPhoto { get; set; }

        public static UserModel LoggedInUser { get; set; }
        public static bool NavRegister { get; set; } = false;
    }
}
