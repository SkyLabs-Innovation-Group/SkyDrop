// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SkyDrop.iOS.Views.Drop
{
	[Register ("DropView")]
	partial class DropView
	{
		[Outlet]
		UIKit.UIActivityIndicatorView ActivityIndicator { get; set; }

		[Outlet]
		UIKit.UIView BarcodeContainer { get; set; }

		[Outlet]
		UIKit.UIView BarcodeMenu { get; set; }

		[Outlet]
		UIKit.UIView CopyLinkButton { get; set; }

		[Outlet]
		UIKit.UICollectionView FilePreviewCollectionView { get; set; }

		[Outlet]
		UIKit.UILabel FileSizeLabel { get; set; }

		[Outlet]
		UIKit.UIView OpenButton { get; set; }

		[Outlet]
		UIKit.UIView ProgressFillArea { get; set; }

		[Outlet]
		UIKit.UIView ReceiveButton { get; set; }

		[Outlet]
		UIKit.UIImageView ReceiveIcon { get; set; }

		[Outlet]
		UIKit.UIView SendButton { get; set; }

		[Outlet]
		UIKit.UIImageView SendIcon { get; set; }

		[Outlet]
		UIKit.UILabel SendLabel { get; set; }

		[Outlet]
		UIKit.UIView SendReceiveButtonsContainer { get; set; }

		[Outlet]
		UIKit.UIView ShareButton { get; set; }

		[Action ("DropViewClickAction:")]
		partial void DropViewClickAction (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (ActivityIndicator != null) {
				ActivityIndicator.Dispose ();
				ActivityIndicator = null;
			}

			if (BarcodeContainer != null) {
				BarcodeContainer.Dispose ();
				BarcodeContainer = null;
			}

			if (BarcodeMenu != null) {
				BarcodeMenu.Dispose ();
				BarcodeMenu = null;
			}

			if (CopyLinkButton != null) {
				CopyLinkButton.Dispose ();
				CopyLinkButton = null;
			}

			if (FilePreviewCollectionView != null) {
				FilePreviewCollectionView.Dispose ();
				FilePreviewCollectionView = null;
			}

			if (OpenButton != null) {
				OpenButton.Dispose ();
				OpenButton = null;
			}

			if (ProgressFillArea != null) {
				ProgressFillArea.Dispose ();
				ProgressFillArea = null;
			}

			if (ReceiveButton != null) {
				ReceiveButton.Dispose ();
				ReceiveButton = null;
			}

			if (ReceiveIcon != null) {
				ReceiveIcon.Dispose ();
				ReceiveIcon = null;
			}

			if (SendButton != null) {
				SendButton.Dispose ();
				SendButton = null;
			}

			if (SendIcon != null) {
				SendIcon.Dispose ();
				SendIcon = null;
			}

			if (SendLabel != null) {
				SendLabel.Dispose ();
				SendLabel = null;
			}

			if (SendReceiveButtonsContainer != null) {
				SendReceiveButtonsContainer.Dispose ();
				SendReceiveButtonsContainer = null;
			}

			if (ShareButton != null) {
				ShareButton.Dispose ();
				ShareButton = null;
			}

			if (FileSizeLabel != null) {
				FileSizeLabel.Dispose ();
				FileSizeLabel = null;
			}
		}
	}
}
