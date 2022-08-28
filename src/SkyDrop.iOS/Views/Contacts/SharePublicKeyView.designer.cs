// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SkyDrop.iOS.Views.Contacts
{
	[Register ("SharePublicKeyView")]
	partial class SharePublicKeyView
	{
		[Outlet]
		UIKit.UIImageView BarcodeImage { get; set; }

		[Outlet]
		UIKit.UILabel ScanAgainButton { get; set; }

		[Outlet]
		UIKit.UIView ScannerContainer { get; set; }

		[Outlet]
		UIKit.UIView ScannerOverlay { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ScannerContainer != null) {
				ScannerContainer.Dispose ();
				ScannerContainer = null;
			}

			if (ScannerOverlay != null) {
				ScannerOverlay.Dispose ();
				ScannerOverlay = null;
			}

			if (ScanAgainButton != null) {
				ScanAgainButton.Dispose ();
				ScanAgainButton = null;
			}

			if (BarcodeImage != null) {
				BarcodeImage.Dispose ();
				BarcodeImage = null;
			}
		}
	}
}
