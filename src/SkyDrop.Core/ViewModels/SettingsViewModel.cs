using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Services;
using Xamarin.Essentials;

namespace SkyDrop.Core.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public bool UploadNotificationsEnabled { get; set; } = true;
        public bool VerifySslCertificates { get; set; } = true;
        public string SkynetPortalLabelText { get; set; } = "Enter a skynet portal to use in the app (default is siasky.net):";

        public IMvxCommand BackCommand { get; set; }
        public IMvxAsyncCommand<string> ValidateAndTrySetSkynetPortalCommand { get; set; }

        /// <summary>
        /// Currently, the best way to verify a Skynet portal that I can think of would be to query for a sky file's metadata.
        /// </summary>
        private const string RandomFileToQueryFor = "AACEA6yg7OM0_gl6_sHx2D7ztt20-g0oXum5GNbCc0ycRg";

        private readonly ISkyDropHttpClientFactory httpClientFactory;
        private readonly IMvxNavigationService navigationService;

        public SettingsViewModel(ISingletonService singletonService,
                                 ISkyDropHttpClientFactory skyDropHttpClientFactory,
                                 IMvxNavigationService navigationService) : base(singletonService)
        {
            this.httpClientFactory = skyDropHttpClientFactory;
            this.navigationService = navigationService;

            Title = "Advanced settings";

            BackCommand = new MvxAsyncCommand(async () => await navigationService.Close(this));
            ValidateAndTrySetSkynetPortalCommand = new MvxAsyncCommand<string>(async url => await ValidateAndTrySetSkynetPortal(url));
        }

        public void Toast(string message)
        {
            singletonService.UserDialogs.Toast(message);
        }

        public override void ViewCreated()
        {
            UploadNotificationsEnabled = Preferences.Get(PreferenceKey.UploadNotificationsEnabled, true);
            VerifySslCertificates = Preferences.Get(PreferenceKey.RequireSecureConnection, true);
            base.ViewCreated();
        }

        private async Task ValidateAndTrySetSkynetPortal(string portalUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(portalUrl))
                {
                    Log.Error("User entered null or empty portal");
                    return;
                }

                portalUrl = FormatPortalUrl(portalUrl);
                var portal = new SkynetPortal(portalUrl);
             
                bool userHasConfirmed = await singletonService.UserDialogs.ConfirmAsync($"Set your portal to {portalUrl} ?");
                if (!userHasConfirmed)
                    return;

                var promptResult = await singletonService.UserDialogs
                    .PromptAsync("Paste your API key if you have one, close if you already entered one for this portal before", "Optional Authentication", "Save", "Close", "", Acr.UserDialogs.InputType.Default);
                portal.UserApiToken = promptResult.Text;

                singletonService.UserDialogs.ShowLoading("Validating portal...");
                bool success = await ValidatePortal(portal);
                singletonService.UserDialogs.HideLoading();

                if (!success)
                    return;

                singletonService.UserDialogs.Toast("Your SkyDrop portal is now set to " + portalUrl);
                SkynetPortal.SelectedPortal = portal;
                // Once the user updates SkynetPortal.SelectedPortal, file downloads and uploads should use their preferred portal
                // If this degrades performance significantly, I think it would be ideal to make toggling between portals:
                // 1) Suggested by the app with a dialog if net is slow,
                // 2) Manually toggleable between saved portals in settings
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

        public Task<bool> ValidatePortal(SkynetPortal portal)
            => singletonService.ApiService.PingPortalForSkylink(RandomFileToQueryFor, portal);

        public void SetUploadNotificationEnabled(bool value)
        {
            UploadNotificationsEnabled = value;
            Preferences.Remove(PreferenceKey.UploadNotificationsEnabled);
            Preferences.Set(PreferenceKey.UploadNotificationsEnabled, value);
        }

        public void SetVerifySslCertificates(bool value)
        {
            VerifySslCertificates = value;
            Preferences.Remove(PreferenceKey.RequireSecureConnection);
            Preferences.Set(PreferenceKey.RequireSecureConnection, value);

            //clients must be rebuilt after changing this setting
            httpClientFactory.ClearCachedClients();
        }

        private string FormatPortalUrl(string portalUrl)
        {
            if (!portalUrl.StartsWith("http"))
                portalUrl = $"https://{portalUrl}";

            if (portalUrl.StartsWith("http://"))
                portalUrl = $"https://{portalUrl.Substring(7)}";

            return portalUrl.TrimEnd('/');
        }

        public void Close()
        {
            navigationService.Close(this);
        }
    }
}
