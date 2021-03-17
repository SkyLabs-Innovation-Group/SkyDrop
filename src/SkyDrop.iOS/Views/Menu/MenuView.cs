using System;
using Acr.UserDialogs;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels.Main;
using UIKit;

namespace SkyDrop.iOS.Views.Menu
{
    [MvxRootPresentation(WrapInNavigationController = true)]
    public partial class MenuView : MvxViewController<MenuViewModel>
    {
        public MenuView() : base("MenuView", null)
        {

        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //TODO: dark green navigation bar with title seems to be covered by another (grey) navigation bar
            NavigationController.Title = "SkyDrop";
            NavigationController.NavigationBar.BackgroundColor = Colors.PrimaryDark.ToNative();

            View.BackgroundColor = Colors.DarkGrey.ToNative();
            FileTransferButton.BackgroundColor = Colors.MidGrey.ToNative();
            SettingsButton.BackgroundColor = Colors.MidGrey.ToNative();
            FileTransferLabel.TextColor = Colors.LightGrey.ToNative();
            SettingsLabel.TextColor = Colors.LightGrey.ToNative();
            FileTransferIcon.TintColor = Colors.LightGrey.ToNative();
            SettingsIcon.TintColor = Colors.LightGrey.ToNative();

            FileTransferButton.Layer.CornerRadius = 8;
            SettingsButton.Layer.CornerRadius = 8;
        }
    }
}

