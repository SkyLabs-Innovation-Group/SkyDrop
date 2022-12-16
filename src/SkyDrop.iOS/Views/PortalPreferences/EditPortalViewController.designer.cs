// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SkyDrop.iOS.Views.PortalPreferences
{
	[Register ("EditPortalViewController")]
	partial class EditPortalViewController
	{
		[Outlet]
		UIKit.UIView DeleteButton { get; set; }

		[Outlet]
		UIKit.UILabel DeleteLabel { get; set; }

		[Outlet]
		UIKit.UIView PasteApiKeyButton { get; set; }

		[Outlet]
		UIKit.UIImageView PasteIcon { get; set; }

		[Outlet]
		UIKit.UITextField PortalApiTokenInput { get; set; }

		[Outlet]
		UIKit.UIView PortalApiTokenInputContainer { get; set; }

		[Outlet]
		UIKit.UITextField PortalNameInput { get; set; }

		[Outlet]
		UIKit.UIView PortalNameInputContainer { get; set; }

		[Outlet]
		UIKit.UITextField PortalUrlInput { get; set; }

		[Outlet]
		UIKit.UIView PortalUrlInputContainer { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (DeleteButton != null) {
				DeleteButton.Dispose ();
				DeleteButton = null;
			}

			if (PasteApiKeyButton != null) {
				PasteApiKeyButton.Dispose ();
				PasteApiKeyButton = null;
			}

			if (PasteIcon != null) {
				PasteIcon.Dispose ();
				PasteIcon = null;
			}

			if (PortalApiTokenInput != null) {
				PortalApiTokenInput.Dispose ();
				PortalApiTokenInput = null;
			}

			if (PortalApiTokenInputContainer != null) {
				PortalApiTokenInputContainer.Dispose ();
				PortalApiTokenInputContainer = null;
			}

			if (PortalNameInput != null) {
				PortalNameInput.Dispose ();
				PortalNameInput = null;
			}

			if (PortalNameInputContainer != null) {
				PortalNameInputContainer.Dispose ();
				PortalNameInputContainer = null;
			}

			if (PortalUrlInput != null) {
				PortalUrlInput.Dispose ();
				PortalUrlInput = null;
			}

			if (PortalUrlInputContainer != null) {
				PortalUrlInputContainer.Dispose ();
				PortalUrlInputContainer = null;
			}

			if (DeleteLabel != null) {
				DeleteLabel.Dispose ();
				DeleteLabel = null;
			}
		}
	}
}
