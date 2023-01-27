﻿using Android.App;
using Android.OS;
using Android.Widget;
using MvvmCross.Commands;
using SkyDrop.Core.ViewModels;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Views.Main
{
    [Activity(Label = "SettingsView")]
    public class SettingsView : BaseActivity<SettingsViewModel>
    {
        private CheckBox uploadNotificationCheckbox, verifySslCheckbox;

        protected override int ActivityLayoutId => Resource.Layout.SettingsView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            BindViews();

            ViewModel.CloseKeyboardCommand = new MvxAsyncCommand(this.HideKeyboard);
        }

        private void BindViews()
        {
            uploadNotificationCheckbox = FindViewById<CheckBox>(Resource.Id.uploadNotificationCheckbox);
            uploadNotificationCheckbox.Visibility = Android.Views.ViewStates.Gone;
            uploadNotificationCheckbox.CheckedChange += (s, e) => ViewModel.SetUploadNotificationEnabled(e.IsChecked);

            verifySslCheckbox = FindViewById<CheckBox>(Resource.Id.verifySslCheckbox);
            verifySslCheckbox.Visibility = Android.Views.ViewStates.Gone;
            verifySslCheckbox.CheckedChange += (s, e) => ViewModel.SetVerifySslCertificates(e.IsChecked);
        }
    }
}