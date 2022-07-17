using System;
using Acr.UserDialogs;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using UIKit;

namespace SkyDrop.iOS.Views.Certificates
{
	[MvxChildPresentationAttribute]
	public partial class ContactsView : MvxViewController<ContactsViewModel>
	{
		public ContactsView() : base ("ContactsView", null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			//setup nav bar
			NavigationController.NavigationBar.TintColor = UIColor.White;
			View.BackgroundColor = Colors.DarkGrey.ToNative();

			var addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add);
			addButton.Clicked += (s, e) =>
			{
				ViewModel.AddContactCommand.Execute();
			};
			NavigationItem.RightBarButtonItem = addButton;

			var set = CreateBindingSet();
			set.Apply();
		}
	}
}


