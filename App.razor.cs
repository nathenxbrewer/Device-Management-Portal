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

namespace ManagedPower.Portal
{
    public partial class App
    {
#if DEBUG
#else
        [Inject] private IHttpClientFactory HttpClientFactory { get; set; }
        [Inject] private Microsoft.Identity.Web.ITokenAcquisition TokenAcquisitionService { get; set; }
        [Inject] private Microsoft.Identity.Web.MicrosoftIdentityConsentAndConditionalAccessHandler ConsentHandler { get; set; }
#endif
        [Inject]
        public HttpClient httpClient { get; set; }
        private HttpClient _httpClient;

        protected override async Task OnInitializedAsync()
        {
#if DEBUG
            Vars.LoggedInUser = new UserModel()
            {
                id = 1,
                displayName = "Admin Mode",
                givenName = "Admin",
                surname = "Mode",
                jobTitle = "AdminMode",
                mail = "Admin@admin.com",
                UserRoles = new List<UserRole>() { new UserRole() { userId = 1, roleId = 4 } }
            };
            Vars.NavRegister = (Vars.LoggedInUser.UserRoles.Select(x => x.roleId).Intersect(new int[] { 1, 2 }).Count() > 0);

#else

            Task.Run(populatesites);
            //This only needs to happen on this page because this app is set via Azure to redirect to this page once the login has been complete. 
            _httpClient = HttpClientFactory.CreateClient();
            var graphToken = "";
            var azureRestToken = "";

            try
            {
                graphToken = await TokenAcquisitionService
                    .GetAccessTokenForUserAsync(new string[] { "User.Read", "Mail.Read" });

                azureRestToken = await TokenAcquisitionService
                    .GetAccessTokenForUserAsync(new string[] { "https://management.azure.com//user_impersonation" });

            }
            catch (Exception ex)
            {
                ConsentHandler.HandleException(ex);
            }

            // make API call
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", graphToken);
            var dataRequest = await _httpClient.GetAsync("https://graph.microsoft.com/beta/me");

            if (dataRequest.IsSuccessStatusCode)
            {
                //cool, now we generate the user model

                var userData = System.Text.Json.JsonDocument.Parse(await dataRequest.Content.ReadAsStreamAsync());
                Vars.LoggedInUser = new Models.UserModel()
                {
                    displayName = userData.RootElement.GetProperty("displayName").GetString(),
                    givenName = userData.RootElement.GetProperty("givenName").GetString(),
                    surname = userData.RootElement.GetProperty("surname").GetString(),
                    mail = userData.RootElement.GetProperty("mail").GetString(),
                    jobTitle = userData.RootElement.GetProperty("jobTitle").GetString(),
                    mobilePhone = userData.RootElement.GetProperty("mobilePhone").GetString(),
                    officeLocation = userData.RootElement.GetProperty("officeLocation").GetString(),

                };

                //populate user photo
                var photoRequest = await _httpClient.GetAsync("https://graph.microsoft.com/v1.0/me/photo/$value");

                if (photoRequest.IsSuccessStatusCode)
                {
                    var buffer = await photoRequest.Content.ReadAsByteArrayAsync();
                    var byteArray = buffer.ToArray();

                    string base64String = Convert.ToBase64String(byteArray);
                    Vars.base64UserPhoto = "data:image/png;base64," + base64String;
                    this.StateHasChanged();
                }
                else
                {
                    Vars.base64UserPhoto = "";
                }

                var matcheduser = httpClient.GetFromJsonAsync<List<UserModel>>("http://cluw12q05.macsstores.com/managedpower/auth/users").Result.FirstOrDefault(x => x.mail.ToLower() == Vars.LoggedInUser.mail.ToLower());
                if (matcheduser != null)
                {
                    Vars.LoggedInUser.profilepic = Vars.base64UserPhoto;
                    Vars.LoggedInUser.id = matcheduser.id;
                    //go update the user in the db to ensure all info is correct. 
                    using (var client = new HttpClient())
                    {
                        var jsonstring = JsonConvert.SerializeObject(Vars.LoggedInUser);
                        var res = client.PostAsync($"http://cluw12q05.macsstores.com/managedpower/auth/users/updateuser/{matcheduser.id}",
                            new StringContent(jsonstring,
                            Encoding.UTF8, "application/json")
                        );
                        try
                        {
                            var result = res.Result.EnsureSuccessStatusCode();
                            if (result.IsSuccessStatusCode)
                            {
                                //user updated successfully
                                //Now get UserRole
                                Vars.LoggedInUser.UserRoles = await httpClient.GetFromJsonAsync<List<UserRole>>($"http://cluw12q05.macsstores.com/managedpower/auth/userroles/getbyuserid/{Vars.LoggedInUser.id}");
                                Vars.NavRegister = (Vars.LoggedInUser.UserRoles.Select(x => x.roleId).Intersect(new int[] { 1, 2 }).Count() > 0);

                            }

                        }
                        catch (Exception e)
                        {

                        }
                    }
                }
                else
                {
                    //user is not on the DB.

                }

            }

#endif



        }
        void populatesites()
        {
            Vars.AllSites = new SiteList().Sites;
        }
    }
    }
