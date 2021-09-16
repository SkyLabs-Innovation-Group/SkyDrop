using System;
using Acr.UserDialogs;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using UIKit;

namespace SkyDrop.iOS.Views.Settings
{
    partial class SettingsView : MvxViewController<SettingsViewModel>
    {
        public SettingsView() : base("SettingsView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var set = CreateBindingSet();
            set.Bind(SetPortalLabel).For(v => v.Text).To(vm => vm.SkynetPortalLabelText);
            set.Apply();

            //setup nav bar
            NavigationController.NavigationBar.BarTintColor = Colors.GradientDark.ToNative();
            NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes()
            {
                ForegroundColor = Colors.White.ToNative()
            };
            View.BackgroundColor = Colors.DarkGrey.ToNative();
            NavigationController.NavigationBar.TintColor = UIColor.White;

            PortalTextView.Text = SkynetPortal.SelectedPortal.ToString();
            PortalTextView.BackgroundColor = UIColor.White;
            PortalTextView.TextColor = UIColor.Black;

            SavePortalButton.BackgroundColor = UIColor.White;
            SavePortalButton.TouchUpInside += async(s, e) =>
            {
                UIApplication.SharedApplication.KeyWindow.EndEditing(true);
                await ViewModel.ValidateAndTrySetSkynetPortal(PortalTextView.Text);
            };

            EnableUploadNotificationsSwitch.On = ViewModel.UploadNotificationsEnabled;
            EnableUploadNotificationsSwitch.ValueChanged += EnableUploadNotificationsSwitch_ValueChanged;
        }

        private void EnableUploadNotificationsSwitch_ValueChanged(object sender, EventArgs e)
        {
            ViewModel.SetUploadNotificationEnabled((sender as UISwitch).On);
        }
    }
}
