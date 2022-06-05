using System;

using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding.Views;
using SkyDrop.Core.DataViewModels;
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
				set.Apply();
			});
		}
	}
}
