using Android.App;
using Android.OS;
using Android.Widget;
using MvvmCross.Commands;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.ViewModels;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Views.Main
{
    [Activity(Label = "SettingsView")]
    public class SettingsView : BaseActivity<SettingsViewModel>
    {
        private EditText portalEditText;
        private Button saveButton;
        private CheckBox uploadNotificationCheckbox, verifySslCheckbox;

        protected override int ActivityLayoutId => Resource.Layout.SettingsView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            BindViews();

            ViewModel.CloseKeyboardCommand = new MvxAsyncCommand(this.HideKeyboard);

            portalEditText.Text = SkynetPortal.SelectedPortal.ToString();
            saveButton.Click += async (s, e) =>
            {
                await this.HideKeyboard();
                await ViewModel.ValidateAndTrySetSkynetPortalCommand.ExecuteAsync(portalEditText.Text);
                portalEditText.Text = SkynetPortal.SelectedPortal.BaseUrl;
            };
        }

        private void BindViews()
        {
            saveButton = FindViewById<Button>(Resource.Id.saveButton);
            portalEditText = FindViewById<EditText>(Resource.Id.skynetPortalEditText);

            uploadNotificationCheckbox = FindViewById<CheckBox>(Resource.Id.uploadNotificationCheckbox);
            uploadNotificationCheckbox.CheckedChange += (s, e) => ViewModel.SetUploadNotificationEnabled(e.IsChecked);

            verifySslCheckbox = FindViewById<CheckBox>(Resource.Id.verifySslCheckbox);
            verifySslCheckbox.CheckedChange += (s, e) => ViewModel.SetVerifySslCertificates(e.IsChecked);
        }
    }
}