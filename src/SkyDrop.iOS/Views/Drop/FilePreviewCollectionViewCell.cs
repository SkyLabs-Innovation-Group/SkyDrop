using System;
using Acr.UserDialogs;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding.Views;
using SkyDrop.Core.Converters;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Utility;
using SkyDrop.iOS.Bindings;
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
                var set = this.CreateBindingSet<FilePreviewCollectionViewCell, StagedFileDvm>();
                set.Bind(FilenameLabel).To(stagedFile => stagedFile.SkyFile.Filename);
                set.Bind(FileExtensionLabel).To(stagedFile => stagedFile.SkyFile.Filename)
                    .WithConversion(FileExtensionConverter.Name);
                set.Bind(PreviewImage).For(LocalImagePreviewBinding.Name).To(stagedFile => stagedFile.SkyFile);
                set.Bind(ContentView).For("Tap").To(stagedFile => stagedFile.TapCommand);
                set.Bind(AddMoreFilesIcon).For("Visible").To(stagedFile => stagedFile.IsMoreFilesButton);
                set.Apply();
            });
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            ImageContainer.BackgroundColor = Colors.MidGrey.ToNative();
            ImageContainer.Layer.CornerRadius = 8;
        }
    }
}