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
            PortalTextView.BackgroundColor = Colors.MidGrey.ToNative();
            PortalTextView.TextColor = UIColor.White;
            PortalTextView.Layer.CornerRadius = 8;

            SavePortalButton.Layer.CornerRadius = 8;
            SavePortalButton.BackgroundColor = UIColor.White;
            SavePortalButton.TouchUpInside += async (s, e) =>
            {
                UIApplication.SharedApplication.KeyWindow.EndEditing(true);
                var formattedPortalUrl = await ViewModel.ValidateAndTrySetSkynetPortal(PortalTextView.Text);
                PortalTextView.Text = formattedPortalUrl;
            };

            EnableUploadNotificationsSwitch.On = ViewModel.UploadNotificationsEnabled;
            EnableUploadNotificationsSwitch.ValueChanged += EnableUploadNotificationsSwitch_ValueChanged;

            PortalTextView.Changed += (s, e) => AdjustTextBoxContentSize(PortalTextView);

            AdjustTextBoxContentSize(PortalTextView);
        }

        /// <summary>
        /// Keeps text input text centered vertically 
        /// </summary>
        private void AdjustTextBoxContentSize(UITextView tv)
        {
            var deadSpace = tv.Bounds.Height - tv.ContentSize.Height;
            var inset = (float)Math.Max(0, deadSpace / 2.0);
            tv.ContentInset = new UIEdgeInsets(inset, tv.ContentInset.Left, inset, tv.ContentInset.Right);
        }

        private void EnableUploadNotificationsSwitch_ValueChanged(object sender, EventArgs e)
        {
            ViewModel.SetUploadNotificationEnabled((sender as UISwitch).On);
        }
    }
}
