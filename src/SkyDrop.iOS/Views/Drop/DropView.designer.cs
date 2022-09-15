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
		UIKit.UIView BarcodeContainer { get; set; }

		[Outlet]
		UIKit.UIImageView BarcodeImage { get; set; }

		[Outlet]
		UIKit.UIView BarcodeMenu { get; set; }

		[Outlet]
		UIKit.UIView CancelButton { get; set; }

		[Outlet]
		UIKit.UIImageView CancelIcon { get; set; }

		[Outlet]
		UIKit.UILabel CancelLabel { get; set; }

		[Outlet]
		UIKit.UIView CopyLinkButton { get; set; }

		[Outlet]
		UIKit.UIView DownloadButton { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView DownloadButtonActivityIndicator { get; set; }

		[Outlet]
		FFImageLoading.Cross.MvxCachedImageView DownloadButtonIcon { get; set; }

		[Outlet]
		UIKit.UIView EncryptButton { get; set; }

		[Outlet]
		UIKit.UIImageView EncryptIcon { get; set; }

		[Outlet]
		UIKit.UILabel EncryptionLabel { get; set; }

		[Outlet]
		UIKit.UICollectionView FilePreviewCollectionView { get; set; }

		[Outlet]
		UIKit.UILabel FileSizeLabel { get; set; }

		[Outlet]
		UIKit.UIImageView FileTypeIcon { get; set; }

		[Outlet]
		UIKit.UIImageView LeftNavDot { get; set; }

		[Outlet]
		UIKit.UIView OpenButton { get; set; }

		[Outlet]
		FFImageLoading.Cross.MvxCachedImageView PreviewImage { get; set; }

		[Outlet]
		UIKit.UIView ProgressFillArea { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView ReceiveActivityIndicator { get; set; }

		[Outlet]
		UIKit.UIView ReceiveButton { get; set; }

		[Outlet]
		UIKit.UIImageView ReceiveIcon { get; set; }

		[Outlet]
		UIKit.UILabel ReceiveLabel { get; set; }

		[Outlet]
		UIKit.UIImageView RightNavDot { get; set; }

		[Outlet]
		UIKit.UILabel SaveFileLabel { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView SendActivityIndicator { get; set; }

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

		[Outlet]
		UIKit.UIView ShowBarcodeButton { get; set; }

		[Outlet]
		UIKit.UIView ShowPreviewButton { get; set; }

		[Outlet]
		UIKit.UIImageView ShowPreviewIcon { get; set; }

		[Outlet]
		UIKit.UILabel UrlLabel { get; set; }

		[Outlet]
		UIKit.UIView UrlLabelContainer { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CancelLabel != null) {
				CancelLabel.Dispose ();
				CancelLabel = null;
			}

			if (CancelIcon != null) {
				CancelIcon.Dispose ();
				CancelIcon = null;
			}

			if (BarcodeContainer != null) {
				BarcodeContainer.Dispose ();
				BarcodeContainer = null;
			}

			if (BarcodeImage != null) {
				BarcodeImage.Dispose ();
				BarcodeImage = null;
			}

			if (BarcodeMenu != null) {
				BarcodeMenu.Dispose ();
				BarcodeMenu = null;
			}

			if (CancelButton != null) {
				CancelButton.Dispose ();
				CancelButton = null;
			}

			if (CopyLinkButton != null) {
				CopyLinkButton.Dispose ();
				CopyLinkButton = null;
			}

			if (DownloadButton != null) {
				DownloadButton.Dispose ();
				DownloadButton = null;
			}

			if (DownloadButtonActivityIndicator != null) {
				DownloadButtonActivityIndicator.Dispose ();
				DownloadButtonActivityIndicator = null;
			}

			if (DownloadButtonIcon != null) {
				DownloadButtonIcon.Dispose ();
				DownloadButtonIcon = null;
			}

			if (EncryptButton != null) {
				EncryptButton.Dispose ();
				EncryptButton = null;
			}

			if (EncryptIcon != null) {
				EncryptIcon.Dispose ();
				EncryptIcon = null;
			}

			if (EncryptionLabel != null) {
				EncryptionLabel.Dispose ();
				EncryptionLabel = null;
			}

			if (FilePreviewCollectionView != null) {
				FilePreviewCollectionView.Dispose ();
				FilePreviewCollectionView = null;
			}

			if (FileSizeLabel != null) {
				FileSizeLabel.Dispose ();
				FileSizeLabel = null;
			}

			if (FileTypeIcon != null) {
				FileTypeIcon.Dispose ();
				FileTypeIcon = null;
			}

			if (LeftNavDot != null) {
				LeftNavDot.Dispose ();
				LeftNavDot = null;
			}

			if (OpenButton != null) {
				OpenButton.Dispose ();
				OpenButton = null;
			}

			if (PreviewImage != null) {
				PreviewImage.Dispose ();
				PreviewImage = null;
			}

			if (ProgressFillArea != null) {
				ProgressFillArea.Dispose ();
				ProgressFillArea = null;
			}

			if (ReceiveActivityIndicator != null) {
				ReceiveActivityIndicator.Dispose ();
				ReceiveActivityIndicator = null;
			}

			if (ReceiveButton != null) {
				ReceiveButton.Dispose ();
				ReceiveButton = null;
			}

			if (ReceiveIcon != null) {
				ReceiveIcon.Dispose ();
				ReceiveIcon = null;
			}

			if (ReceiveLabel != null) {
				ReceiveLabel.Dispose ();
				ReceiveLabel = null;
			}

			if (RightNavDot != null) {
				RightNavDot.Dispose ();
				RightNavDot = null;
			}

			if (SaveFileLabel != null) {
				SaveFileLabel.Dispose ();
				SaveFileLabel = null;
			}

			if (SendActivityIndicator != null) {
				SendActivityIndicator.Dispose ();
				SendActivityIndicator = null;
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

			if (ShowBarcodeButton != null) {
				ShowBarcodeButton.Dispose ();
				ShowBarcodeButton = null;
			}

			if (ShowPreviewButton != null) {
				ShowPreviewButton.Dispose ();
				ShowPreviewButton = null;
			}

			if (ShowPreviewIcon != null) {
				ShowPreviewIcon.Dispose ();
				ShowPreviewIcon = null;
			}

			if (UrlLabel != null) {
				UrlLabel.Dispose ();
				UrlLabel = null;
			}

			if (UrlLabelContainer != null) {
				UrlLabelContainer.Dispose ();
				UrlLabelContainer = null;
			}
		}
	}
}
