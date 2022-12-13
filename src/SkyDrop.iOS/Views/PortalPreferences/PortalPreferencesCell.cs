using System;
using Acr.UserDialogs;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding.Views;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Utility;
using UIKit;

namespace SkyDrop.iOS.Views.PortalPreferences
{
    public partial class PortalPreferencesCell : MvxTableViewCell
    {
        public static readonly NSString Key = new NSString("PortalPreferencesCell");
        public static readonly UINib Nib;

        static PortalPreferencesCell()
        {
            Nib = UINib.FromName("PortalPreferencesCell", NSBundle.MainBundle);
        }

        protected PortalPreferencesCell(IntPtr handle) : base(handle)
        {
            this.DelayBind(() =>
            {
                var set = this.CreateBindingSet<PortalPreferencesCell, SkynetPortalDvm>();
                set.Bind(PortalNameLabel).For(t => t.Text).To(vm => vm.Name);
                set.Bind(PortalUrlLabel).For(t => t.Text).To(vm => vm.BaseUrl);
                set.Bind(ContentView).For("Tap").To(vm => vm.TapCommand);
                set.Apply();
            });
        }

        [Export("awakeFromNib")]
        public void AwakeFromNib()
        {
            Icon.TintColor = Colors.LightGrey.ToNative();
        }
    }
}