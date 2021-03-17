using System;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Platforms.Ios.Views;
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


        }
    }
}

