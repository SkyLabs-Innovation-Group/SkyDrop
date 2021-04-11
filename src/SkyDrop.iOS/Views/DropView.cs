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

            var set = CreateBindingSet();

            set.Bind(SendButton).For("Tap").To(vm => vm.SendCommand);

            set.Bind(ReceiveButton).For("Tap").To(vm => vm.ReceiveCommand);

            set.Apply();

            // TODO implement functions for send/receive buttons
            //ViewModel.SelectFileAsyncFunc = async () => await Utils.SelectFile(this);
            //ViewModel.SelectImageAsyncFunc = async () => await Utils.SelectImage(this);

            //TODO: dark green navigation bar with title seems to be covered by another (grey) navigation bar
            NavigationController.Title = "SkyDrop";
            NavigationController.NavigationBar.BackgroundColor = Colors.GradientDark.ToNative();

            View.BackgroundColor = Colors.DarkGrey.ToNative();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}

