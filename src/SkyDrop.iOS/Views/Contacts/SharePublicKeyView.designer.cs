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
		UIKit.UILabel HintLabel { get; set; }

		[Outlet]
		UIKit.UIView ScannerContainer { get; set; }

		[Outlet]
		UIKit.UIView ScannerOverlay { get; set; }

		[Outlet]
		UIKit.UIImageView StatusIcon { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (BarcodeImage != null) {
				BarcodeImage.Dispose ();
				BarcodeImage = null;
			}

			if (ScannerContainer != null) {
				ScannerContainer.Dispose ();
				ScannerContainer = null;
			}

			if (ScannerOverlay != null) {
				ScannerOverlay.Dispose ();
				ScannerOverlay = null;
			}

			if (HintLabel != null) {
				HintLabel.Dispose ();
				HintLabel = null;
			}

			if (StatusIcon != null) {
				StatusIcon.Dispose ();
				StatusIcon = null;
			}
		}
	}
}
