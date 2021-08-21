using System;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.ViewModels;
using UIKit;

namespace SkyDrop.iOS.Views.Drop
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

            SavePortalButton.BackgroundColor = UIColor.White;
            SavePortalButton.TouchUpInside += async(s, e) =>
            {
                await ViewModel.ValidateAndTrySetSkynetPortal(PortalTextView.Text);
            };
        }
    }
}
