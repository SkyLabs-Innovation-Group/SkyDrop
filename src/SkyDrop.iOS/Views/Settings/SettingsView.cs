using System;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using UIKit;

namespace SkyDrop.iOS.Views.Settings
{
    partial class SettingsView : BaseViewController<SettingsViewModel>
    {
        public SettingsView() : base("SettingsView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            AddBackButton(() => ViewModel.Close());

            ViewModel.CloseKeyboardCommand = new MvxCommand(() => View.EndEditing(true));

            var set = CreateBindingSet();
            set.Bind(SetPortalLabel).For(v => v.Text).To(vm => vm.SkynetPortalLabelText);
            set.Bind(NameTextView).For(v => v.Text).To(vm => vm.DeviceName);
            set.Bind(SetNameButton).To(vm => vm.SetDeviceNameCommand);
            set.Bind(this).For(t => t.Title).To(vm => vm.Title);
            set.Apply();

            StyleTextInput(PortalTextView);
            StyleTextInput(NameTextView);

            StyleButton(SavePortalButton);
            StyleButton(SetNameButton);

            PortalTextView.Text = SkynetPortal.SelectedPortal.ToString();
            SavePortalButton.TouchUpInside += async (s, e) =>
            {
                UIApplication.SharedApplication.KeyWindow.EndEditing(true);
                await ViewModel.ValidateAndTrySetSkynetPortalCommand.ExecuteAsync(PortalTextView.Text);
                PortalTextView.Text = SkynetPortal.SelectedPortal.BaseUrl;
            };

            EnableUploadNotificationsSwitch.On = ViewModel.UploadNotificationsEnabled;
            EnableUploadNotificationsSwitch.ValueChanged += EnableUploadNotificationsSwitch_ValueChanged;
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

        private void StyleTextInput(UITextView textView)
        {
            textView.BackgroundColor = Colors.MidGrey.ToNative();
            textView.TextColor = UIColor.White;
            textView.Layer.CornerRadius = 8;
            textView.Changed += (s, e) => AdjustTextBoxContentSize(PortalTextView);
            AdjustTextBoxContentSize(textView);
        }

        private void StyleButton(UIButton button)
        {
            button.Layer.CornerRadius = 8;
            button.BackgroundColor = UIColor.White;
        }
    }
}
