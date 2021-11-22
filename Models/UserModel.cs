using System.Collections.Generic;

namespace ManagedPower.Portal.Models
{
    public class UserModel
    {
        public int id { get; set; }
        public string microsoftId { get; set; }
        public string displayName { get; set; }
        public string givenName { get; set; }
        public string jobTitle { get; set; }
        public string mail { get; set; }
        public string mobilePhone { get; set; }
        public string officeLocation { get; set; }
        public string preferredLanguage { get; set; }
        public string surname { get; set; }
        public string userPrincipalName { get; set; }
        public string profilepic { get; set; }

        public List<UserRole> UserRoles { get; set; }
    }
}
