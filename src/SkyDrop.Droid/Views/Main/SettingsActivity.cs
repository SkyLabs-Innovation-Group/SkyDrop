using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyDrop.Droid.Views.Main
{
    [Activity(Label = "SettingsActivity")]
    public class SettingsActivity : BaseActivity<SettingsViewModel>
    {
        /// <summary>
        /// Currently, the best way to verify a Skynet portal that I can think of would be to query for a sky file's metadata.
        /// </summary>
        private const string RandomFileToQueryFor = "AACEA6yg7OM0_gl6_sHx2D7ztt20-g0oXum5GNbCc0ycRg";

        protected override int ActivityLayoutId => Resource.Layout.SettingsActivity;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var saveButton = FindViewById<Button>(Resource.Id.saveButton);

            saveButton.Click += async (s, e) => await ValidateAndSetSkynetPortal();
        }

        protected async Task ValidateAndSetSkynetPortal()
        {
            try
            {
                var portalEditText = FindViewById<EditText>(Resource.Id.skynetPortalEditText);

                string portalUrl = portalEditText.Text;
                if (string.IsNullOrEmpty(portalUrl))
                {
                    Log.Error("User entered null or empty portal, setting to siasky.net");
                }
                else
                {
                    var portal = new SkynetPortal(portalUrl);
                    bool success = await ViewModel.singletonService.ApiService.PingPortalForSkylink(RandomFileToQueryFor, portal);

                    if (success)
                    {
                        bool userHasConfirmed = await ViewModel.singletonService.UserDialogs.ConfirmAsync($"Set your portal to {portalUrl} ?");

                        if (userHasConfirmed)
                            SkynetPortal.SelectedPortal = portal;

                        // Once the user updates SkynetPortal.SelectedPortal, file downloads and uploads should use their preferred portal
                        // If this degrades performance significantly, I think it would be ideal to make toggling between portals:
                        // 1) Suggested by the app with a dialog if net is slow,
                        // 2) Manually toggleable between saved portals in settings
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
    }
}