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
	public partial class FileTableViewCell : MvxTableViewCell
	{
		public static readonly NSString Key = new NSString ("FileTableViewCell");
		public static readonly UINib Nib;

		static FileTableViewCell ()
		{
			Nib = UINib.FromName ("FileTableViewCell", NSBundle.MainBundle);
		}

		protected FileTableViewCell (IntPtr handle) : base (handle)
		{
			this.DelayBind(() =>
			{
				var set = this.CreateBindingSet<FileTableViewCell, SkyFileDVM>();
				set.Bind(FileNameLabel).To(vm => vm.SkyFile.Filename);
				set.Bind(SelectedIndicatorView).For(i => i.BackgroundColor).To(vm => vm.SelectionIndicatorColor).WithConversion("NativeColor");
				set.Bind(SelectedIndicatorView).For("Visible").To(vm => vm.IsSelectionActive);
				set.Bind(SelectedIndicatorInnerView).For("Visible").To(vm => vm.IsSelected);
				set.Bind(IconImage).For(i => i.Hidden).To(vm => vm.IsSelectionActive);
				set.Bind(ContentView).For("Tap").To(vm => vm.TapCommand);
				set.Bind(ContentView).For("LongPress").To(vm => vm.LongPressCommand);
				set.Apply();
			});
		}

        [Export("awakeFromNib")]
        public void AwakeFromNib()
        {
			ContainerView.BackgroundColor = Colors.MidGrey.ToNative();
			SelectedIndicatorView.BackgroundColor = Colors.LightGrey.ToNative();
        }
    }
}
