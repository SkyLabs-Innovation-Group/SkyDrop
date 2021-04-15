using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Foundation;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels.Main;
using UIKit;

namespace SkyDrop.iOS.Views.Drop
{
    [MvxRootPresentation(WrapInNavigationController = true)]
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
            
            NavigationController.Title = "SkyDrop";

            var barColor =  Colors.GradientDeepBlue.ToNative();
            NavigationController.NavigationBar.BarTintColor = barColor;
            
            View.BackgroundColor = Colors.DarkGrey.ToNative();
            
            var set = CreateBindingSet();

            set.Bind(SendButton).For("Tap").To(vm => vm.SendCommand);

            set.Bind(ReceiveButton).For("Tap").To(vm => vm.ReceiveCommand);
            
            // set.Bind(NavigationController.nav).For(n => n.NavigationBar)

            set.Apply();

            // TODO implement functions for send/receive buttons
            //ViewModel.SelectFileAsyncFunc = async () => await Utils.SelectFile(this);
            //ViewModel.SelectImageAsyncFunc = async () => await Utils.SelectImage(this);


        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}

