using System;
using SkyDrop.iOS.Bindings;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding.Views;
using SkyDrop.Core.DataModels;
using UIKit;
using SkyDrop.Core.Utility;
using Acr.UserDialogs;
using SkyDrop.Core.Converters;

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
                set.Bind(FileExtensionLabel).To(skyFile => skyFile.Filename).WithConversion(FileExtensionConverter.Name);
                set.Bind(PreviewImage).For(ByteArrayImageViewBinding.Name).To(skyFile => skyFile.Data);
                set.Apply();
            });
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            ImageContainer.BackgroundColor = Colors.GradientTurqouise.ToNative();
            ImageContainer.Layer.CornerRadius = 8;
        }
    }
}
