using System;

using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding.Views;
using SkyDrop.Core.DataModels;
using UIKit;

namespace SkyDrop.iOS.Views.Drop
{
    public partial class FilePreviewCollectionViewCell : MvxCollectionViewCell
    {
        public static readonly NSString Key = new NSString("FilePreviewCollectionViewCell");
        public static readonly UINib Nib;

        static FilePreviewCollectionViewCell()
        {
            Nib = UINib.FromName(Key, NSBundle.MainBundle);
        }

        protected FilePreviewCollectionViewCell(IntPtr handle) : base(handle)
        {
            this.DelayBind(() =>
            {
                var set = this.CreateBindingSet<FilePreviewCollectionViewCell, SkyFile>();
                set.Bind(FilenameLabel).To(skyFile => skyFile.Filename);
                set.Apply();
            });
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            ImageContainer.Layer.CornerRadius = 8;
        }
    }
}
