using System;

using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding.Views;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
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
				set.Bind(NameLabel).To(vm => vm.Contact.Name);
				set.Apply();
			});
		}
	}
}
