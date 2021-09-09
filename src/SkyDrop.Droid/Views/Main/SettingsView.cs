using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
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
    [Activity(Label = "SettingsView")]
    public class SettingsView : BaseActivity<SettingsViewModel>
    {
        private Button saveButton;
        private EditText portalEditText;
        private CheckBox uploadNotificationCheckbox;


        protected override int ActivityLayoutId => Resource.Layout.SettingsActivity;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            BindViews();

            portalEditText.Text = SkynetPortal.SelectedPortal.ToString();
            saveButton.Click += async (s, e) =>
            {
                HideKeyboard();
                await ViewModel.ValidateAndTrySetSkynetPortal(portalEditText.Text);
            };
        }

        private void BindViews()
        {
            saveButton = FindViewById<Button>(Resource.Id.saveButton);
            portalEditText = FindViewById<EditText>(Resource.Id.skynetPortalEditText);
            uploadNotificationCheckbox = FindViewById<CheckBox>(Resource.Id.uploadNotificationCheckbox);
            uploadNotificationCheckbox.CheckedChange += 
                (s, e) => ViewModel.SetUploadNotificationEnabled(e.IsChecked);
        }

        public void HideKeyboard()
        {
            try
            {
                var inputMethodManager = GetSystemService(InputMethodService) as InputMethodManager;
                if (inputMethodManager != null)
                {
                    var token = CurrentFocus?.WindowToken;
                    inputMethodManager.HideSoftInputFromWindow(token, HideSoftInputFlags.None);

                    Window.DecorView.ClearFocus();
                }
            }
            catch(Exception ex)
            {
                Log.Exception(ex);
            }
        }
    }
}