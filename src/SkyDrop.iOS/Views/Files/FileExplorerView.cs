using System;
using Foundation;
using UIKit;

namespace SkyDrop.iOS.Views.Files
{
    public partial class FileExplorerView : UIView
    {
        public static readonly NSString Key = new NSString("FileExplorerView");
        public static readonly UINib Nib;

        static FileExplorerView()
        {
            Nib = UINib.FromName("FileExplorerView", NSBundle.MainBundle);
        }

        protected FileExplorerView(IntPtr handle) : base(handle)
        {
        }

        public static FileExplorerView CreateView()
        {
            return (FileExplorerView)Nib.Instantiate(null, null)[0];
        }
    }
}

