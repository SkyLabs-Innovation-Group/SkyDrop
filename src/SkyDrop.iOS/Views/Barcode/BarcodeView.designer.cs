// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SkyDrop.iOS.Views.Barcode
{
	[Register ("BarcodeView")]
	partial class BarcodeView
	{
		[Outlet]
		UIKit.UIView BarcodeContainer { get; set; }

		[Outlet]
		UIKit.UIImageView BarcodeImage { get; set; }

		[Outlet]
		UIKit.UIButton OkButton { get; set; }

		[Outlet]
		UIKit.UITextField TextInput { get; set; }

		[Outlet]
		UIKit.UIView TextInputContainer { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (BarcodeContainer != null) {
				BarcodeContainer.Dispose ();
				BarcodeContainer = null;
			}

			if (BarcodeImage != null) {
				BarcodeImage.Dispose ();
				BarcodeImage = null;
			}

			if (TextInput != null) {
				TextInput.Dispose ();
				TextInput = null;
			}

			if (TextInputContainer != null) {
				TextInputContainer.Dispose ();
				TextInputContainer = null;
			}

			if (OkButton != null) {
				OkButton.Dispose ();
				OkButton = null;
			}
		}
	}
}
