using System;
using Acr.UserDialogs;
using Foundation;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.Utility;
using CoreGraphics;
using Foundation;
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
        
        protected void AddBackButton(Action backAction)
        {
            var backImage = UIImage.FromBundle("ic_back");
            var backBtnImage = UIImage.FromImage(backImage.CGImage, backImage.CurrentScale, UIImageOrientation.Up);

            var backBtn = new UIButton(UIButtonType.System)
            {
                HorizontalAlignment = UIControlContentHorizontalAlignment.Left
            };

            backBtn.SetImage(backBtnImage, UIControlState.Normal);

            backBtn.Frame = new CGRect(0, 0,
                UIScreen.MainScreen.Bounds.Width / 9,
                NavigationController.NavigationBar.Frame.Height);

            backBtn.TouchUpInside += (sender, e) => backAction?.Invoke();
            backBtn.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;

            var fixedSpace = new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace)
            {
                Width = -16f
            };

            var backButtonItem = new UIBarButtonItem("", UIBarButtonItemStyle.Plain, null)
            {
                CustomView = backBtn
            };

            NavigationController.TopViewController.NavigationItem.LeftBarButtonItems = new[] { fixedSpace, backButtonItem };
        }
    }
}

