using System;
using Acr.UserDialogs;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding.Views;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Utility;
using UIKit;

namespace SkyDrop.iOS.Views.Files
{
    public partial class FolderCell : MvxTableViewCell
    {
        public static readonly NSString Key = new NSString("FolderCell");
        public static readonly UINib Nib;

        static FolderCell()
        {
            Nib = UINib.FromName("FolderCell", NSBundle.MainBundle);
        }

        protected FolderCell(IntPtr handle) : base(handle)
        {
            this.DelayBind(() =>
            {
                var set = this.CreateBindingSet<FolderCell, FolderDvm>();
                set.Bind(NameLabel).To(vm => vm.Name);
                set.Bind(ContentView).For("Tap").To(vm => vm.TapCommand);
                set.Bind(ContentView).For("LongPress").To(vm => vm.LongPressCommand);
                set.Bind(SelectedIndicatorView).For(i => i.BackgroundColor).To(vm => vm.SelectionIndicatorColor)
                    .WithConversion("NativeColor");
                set.Bind(SelectedIndicatorView).For("Visible").To(vm => vm.IsSelectionActive);
                set.Bind(SelectedIndicatorInnerView).For("Visible").To(vm => vm.IsSelected);
                set.Apply();
            });
        }

        [Export("awakeFromNib")]
        public void AwakeFromNib()
        {
            ContainerView.BackgroundColor = Colors.MidGrey.ToNative();
            SelectionStyle = UITableViewCellSelectionStyle.None;
        }
    }
}