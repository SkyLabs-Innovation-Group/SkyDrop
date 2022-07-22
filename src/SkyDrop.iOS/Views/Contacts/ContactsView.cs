using System;
using Acr.UserDialogs;
using CoreGraphics;
using MvvmCross.Platforms.Ios.Binding.Views;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using SkyDrop.iOS.Views.Contacts;
using UIKit;

namespace SkyDrop.iOS.Views.Certificates
{
	[MvxChildPresentation]
	public partial class ContactsView : BaseViewController<ContactsViewModel>
	{
		public ContactsView() : base("ContactsView", null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			PreferredContentSize = new CGSize(300, 300);

			ContactsTableView.BackgroundColor = Colors.DarkGrey.ToNative();

			var addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add);
			addButton.Clicked += (s, e) => ViewModel.AddContactCommand.Execute();
			var shareButton = new UIBarButtonItem { Image = UIImage.FromBundle("ic_qr") };
            shareButton.Clicked += (s, e) => ViewModel.SharePublicKeyCommand.Execute();
			NavigationItem.RightBarButtonItems = new[]{ addButton, shareButton };

			var source = new MvxSimpleTableViewSource(ContactsTableView, ContactCell.Key);
			ContactsTableView.Source = source;
			ContactsTableView.AllowsSelection = false;
			ContactsTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

			var set = CreateBindingSet();
			set.Bind(source).For(s => s.ItemsSource).To(vm => vm.Contacts);
			set.Bind(ErrorView).For("Visible").To(vm => vm.IsNoContacts);
			set.Bind(this).For(s => s.Title).To(vm => vm.Title);
			set.Apply();
		}
    }
}


