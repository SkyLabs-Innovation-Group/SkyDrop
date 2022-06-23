using System;
using Acr.UserDialogs;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using UIKit;

namespace SkyDrop.iOS.Views.PortalPreferences
{
	[MvxChildPresentationAttribute]
	public partial class PortalPreferencesView : MvxViewController<PortalPreferencesViewModel>
	{
		public PortalPreferencesView() : base ("PortalPreferencesView", null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			NavigationController.NavigationBar.TintColor = UIColor.White;
			View.BackgroundColor = Colors.DarkGrey.ToNative();

			var set = CreateBindingSet();
			set.Bind(this).For(t => t.Title).To(vm => vm.Title);
			set.Apply();
		}
	}
}


