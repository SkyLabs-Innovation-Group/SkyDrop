using System;
using Acr.UserDialogs;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding.Views;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Utility;
using UIKit;

namespace SkyDrop.iOS.Views.Contacts
{
	public partial class ContactCell : MvxTableViewCell
	{
		public static readonly NSString Key = new NSString("ContactCell");
		public static readonly UINib Nib;

		static ContactCell()
		{
			Nib = UINib.FromName("ContactCell", NSBundle.MainBundle);
		}

		protected ContactCell(IntPtr handle) : base(handle)
		{
			this.DelayBind(() =>
			{
				var set = this.CreateBindingSet<ContactCell, ContactDVM>();
				set.Bind(NameLabel).To(vm => vm.Name);
				set.Bind(ContentView).For("Tap").To(vm => vm.TapCommand);
                set.Bind(DeleteButton).For("Tap").To(vm => vm.DeleteCommand);
                set.Bind(RenameButton).For("Tap").To(vm => vm.RenameCommand);
                set.Bind(this).For(t => t.IconImage).To(vm => vm);
				set.Apply();
			});
		}

		[Export("awakeFromNib")]
		public void AwakeFromNib()
		{
			ContentView.BackgroundColor = Colors.DarkGrey.ToNative();
			ContainerView.BackgroundColor = Colors.MidGrey.ToNative();
		}

		public IContactItem IconImage
		{
			get => null;
			set
			{
				//show world icon on first cell
				var icon = value is AnyoneWithTheLinkItem ? "ic_world" : "ic_key";
				Icon.Image = UIImage.FromBundle(icon);

				//hide delete button for top item
				DeleteButton.Hidden = value is AnyoneWithTheLinkItem;
			}
		}
	}
}
