// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SkyDrop.iOS.Views.Files
{
	[Register ("FileCollectionViewCell")]
	partial class FileCollectionViewCell
	{
		[Outlet]
		UIKit.UIView BottomPanel { get; set; }

		[Outlet]
		UIKit.UIView ContainerView { get; set; }

		[Outlet]
		UIKit.UILabel FileNameLabel { get; set; }

		[Outlet]
		UIKit.UIView InnerView { get; set; }

		[Outlet]
		FFImageLoading.Cross.MvxCachedImageView PreviewImage { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (BottomPanel != null) {
				BottomPanel.Dispose ();
				BottomPanel = null;
			}

			if (ContainerView != null) {
				ContainerView.Dispose ();
				ContainerView = null;
			}

			if (FileNameLabel != null) {
				FileNameLabel.Dispose ();
				FileNameLabel = null;
			}

			if (InnerView != null) {
				InnerView.Dispose ();
				InnerView = null;
			}

			if (PreviewImage != null) {
				PreviewImage.Dispose ();
				PreviewImage = null;
			}
		}
	}
}
