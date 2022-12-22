using System;
using Acr.UserDialogs;
using MvvmCross.Commands;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using UIKit;

namespace SkyDrop.iOS.Views.Settings
{
    internal partial class SettingsView : BaseViewController<SettingsViewModel>
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
            set.Bind(NameTextView).For(v => v.Text).To(vm => vm.DeviceName);
            set.Bind(SetNameButton).To(vm => vm.SetDeviceNameCommand);
            set.Bind(OnboardingButton).To(vm => vm.ViewOnboardingCommand);
            set.Bind(this).For(t => t.Title).To(vm => vm.Title);
            set.Apply();

            StyleTextInput(NameTextView);

            StyleButton(SetNameButton);
            StyleButton(OnboardingButton);

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
            textView.Changed += (s, e) => AdjustTextBoxContentSize(textView);
            AdjustTextBoxContentSize(textView);
        }
    }
}