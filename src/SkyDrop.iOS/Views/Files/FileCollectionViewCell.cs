using System;
using Acr.UserDialogs;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding.Views;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Utility;
using UIKit;

namespace SkyDrop.iOS.Views.Files
{
    public partial class FileCollectionViewCell : MvxCollectionViewCell
    {
        public static readonly NSString Key = new NSString("FileCollectionViewCell");
        public static readonly UINib Nib;

        static FileCollectionViewCell()
        {
            Nib = UINib.FromName("FileCollectionViewCell", NSBundle.MainBundle);
        }

        protected FileCollectionViewCell(IntPtr handle) : base(handle)
        {
            this.DelayBind(() =>
            {
                var set = this.CreateBindingSet<FileCollectionViewCell, SkyFileDVM>();
                set.Bind(FileNameLabel).To(vm => vm.SkyFile.Filename);
                set.Apply();
            });
        }

        [Export("awakeFromNib")]
        public void AwakeFromNib()
        {
            ContainerView.BackgroundColor = Colors.DarkGrey.ToNative();
            InnerView.BackgroundColor = Colors.DarkGrey.ToNative();
            BottomPanel.BackgroundColor = Colors.MidGrey.ToNative();

            InnerView.Layer.CornerRadius = 8;
            InnerView.Layer.BorderColor = Colors.MidGrey.ToNative().CGColor;
            InnerView.Layer.BorderWidth = 1;
            InnerView.Layer.MasksToBounds = true;
        }
    }
}
