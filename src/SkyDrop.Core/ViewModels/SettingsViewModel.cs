using SkyDrop.Core.Services.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkyDrop.Core.DataModels;

namespace SkyDrop.Core.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        /// <summary>
        /// Currently, the best way to verify a Skynet portal that I can think of would be to query for a sky file's metadata.
        /// </summary>
        private const string RandomFileToQueryFor = "AACEA6yg7OM0_gl6_sHx2D7ztt20-g0oXum5GNbCc0ycRg";

        public bool UploadNotificationsEnabled { get; set; } = true;
    
        
        public string SkynetPortalLabelText { get; set; }

        public SettingsViewModel(ISingletonService singletonService) : base(singletonService)
        {
            Title = "Advanced settings";
        }

        public override Task Initialize()
        {
            UploadNotificationsEnabled = Preferences.Get(PreferenceKey.UploadNotificationsEnabled, true);
            
            return base.Initialize();
        }

        public void Toast(string message)
        {
            singletonService.UserDialogs.Toast(message);
        }
        
        public override void ViewAppearing()
        {
            Toast("VM ViewAppearing()");
            
            SkynetPortalLabelText = "Enter a skynet portal to use in the app (default is siasky.net):";
            base.ViewAppearing();
        }

        public async Task ValidateAndTrySetSkynetPortal(string portalUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(portalUrl))
                {
                    Log.Error("User entered null or empty portal, setting to siasky.net");
                }
                else
                {
                    if (!portalUrl.StartsWith("http"))
                        portalUrl = $"https://{portalUrl}";
                    
                    var portal = new SkynetPortal(portalUrl);
                    bool success = await singletonService.ApiService.PingPortalForSkylink(RandomFileToQueryFor, portal);

                    if (success)
                    {
                        bool userHasConfirmed =
                            await singletonService.UserDialogs.ConfirmAsync($"Set your portal to {portalUrl} ?");

                        if (userHasConfirmed)
                            SkynetPortal.SelectedPortal = portal;

                        singletonService.UserDialogs.Toast("Your SkyDrop portal is now set to " + portalUrl);
                        // Once the user updates SkynetPortal.SelectedPortal, file downloads and uploads should use their preferred portal
                        // If this degrades performance significantly, I think it would be ideal to make toggling between portals:
                        // 1) Suggested by the app with a dialog if net is slow,
                        // 2) Manually toggleable between saved portals in settings
                    }
                }
            }
            catch (UriFormatException)
            {
                singletonService.UserDialogs.Toast("You must enter a valid URL format");
            }
            catch (Exception ex)
            {
                singletonService.UserDialogs.Toast("Error - couldn't reach portal " + portalUrl);
                Log.Exception(ex);
            }
        }

        public void SetUploadNotificationEnabled(bool value)
        {
            UploadNotificationsEnabled = value;
            Preferences.Remove(PreferenceKey.UploadNotificationsEnabled);
            Preferences.Set(PreferenceKey.UploadNotificationsEnabled, value);
        }
    }
}
