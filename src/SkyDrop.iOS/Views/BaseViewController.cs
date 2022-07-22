using System;
using Acr.UserDialogs;
using Foundation;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using UIKit;

namespace SkyDrop.iOS.Views
{
	public class BaseViewController<T> : MvxViewController<T> where T : BaseViewModel
	{
		public BaseViewController(string name, NSBundle bundle) : base(name, bundle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			Init();
		}

        private void Init()
        {
			//setup nav bar
			NavigationController.NavigationBar.TintColor = UIColor.White;
			NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes()
			{
				ForegroundColor = Colors.White.ToNative()
			};

			//set background color
			View.BackgroundColor = Colors.DarkGrey.ToNative();
		}
	}
}

