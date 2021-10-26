using System;

using Foundation;
using UIKit;

namespace SkyDrop.iOS.Views.Files
{
    public partial class FileCollectionViewCell : UICollectionViewCell
    {
        public static readonly NSString Key = new NSString("FileCollectionViewCell");
        public static readonly UINib Nib;

        static FileCollectionViewCell()
        {
            Nib = UINib.FromName("FileCollectionViewCell", NSBundle.MainBundle);
        }

        protected FileCollectionViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }
    }
}
