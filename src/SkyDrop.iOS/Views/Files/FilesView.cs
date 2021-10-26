using System;
using MvvmCross.Platforms.Ios.Views;
using SkyDrop.Core.ViewModels.Main;
using UIKit;

namespace SkyDrop.iOS.Views.Files
{
    public partial class FilesView : MvxViewController<FilesViewModel>
    {
        public FilesView() : base("FilesView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}

