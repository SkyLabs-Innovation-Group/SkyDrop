// WARNING
//
// This file has been generated automatically by Rider IDE
//   to store outlets and actions made in Xcode.
// If it is removed, they will be lost.
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
		UIKit.UITextView PortalApiTokenTextView { get; set; }

		[Outlet]
		UIKit.UITextView PortalNameTextView { get; set; }

		[Outlet]
		UIKit.UITextView PortalUrlTextView { get; set; }

		[Outlet]
		UIKit.UIButton SaveButton { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (PortalNameTextView != null) {
				PortalNameTextView.Dispose ();
				PortalNameTextView = null;
			}

			if (PortalUrlTextView != null) {
				PortalUrlTextView.Dispose ();
				PortalUrlTextView = null;
			}

			if (PortalApiTokenTextView != null) {
				PortalApiTokenTextView.Dispose ();
				PortalApiTokenTextView = null;
			}

			if (SaveButton != null) {
				SaveButton.Dispose ();
				SaveButton = null;
			}

		}
	}
}
