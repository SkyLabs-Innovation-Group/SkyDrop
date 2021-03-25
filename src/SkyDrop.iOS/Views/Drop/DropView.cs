using System;
using Acr.UserDialogs;
using Foundation;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels.Main;
using UIKit;

namespace SkyDrop.iOS.Views.Drop
{
    public partial class DropView : MvxViewController<DropViewModel>
    {
        public DropView() : base("DropView", null)
        {
        }

        partial void DropViewClickAction(NSObject sender)
        {
            try
            {
                ViewModel.NavToSettingsCommand.Execute();
            }
            catch (Exception ex)
            {
                ViewModel.Log.Exception(ex);
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //TODO: dark green navigation bar with title seems to be covered by another (grey) navigation bar
            NavigationController.Title = "SkyDrop";
            NavigationController.NavigationBar.BackgroundColor = Colors.PrimaryDark.ToNative();

            View.BackgroundColor = Colors.DarkGrey.ToNative();

            SendButton.AddGestureRecognizer(new UIGestureRecognizer(SendButtonTapped));
            ReceiveButton.AddGestureRecognizer(new UIGestureRecognizer(ReceiveButtonTapped));
        }

        private void ReceiveButtonTapped()
        {
            ViewModel.Log.Trace("ReceiveButton pressed");
        }

        public void SendButtonTapped()
        {
            ViewModel.Log.Trace("SendButton pressed");
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}

