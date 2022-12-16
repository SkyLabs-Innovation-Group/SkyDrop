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
	[Register ("PortalPreferencesCell")]
	partial class PortalPreferencesCell
	{
		[Outlet]
		UIKit.UIImageView DownButton { get; set; }

		[Outlet]
		UIKit.UIImageView Icon { get; set; }

		[Outlet]
		UIKit.UILabel PortalNameLabel { get; set; }

		[Outlet]
		UIKit.UILabel PortalUrlLabel { get; set; }

		[Outlet]
		UIKit.UIImageView UpButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (Icon != null) {
				Icon.Dispose ();
				Icon = null;
			}

			if (PortalNameLabel != null) {
				PortalNameLabel.Dispose ();
				PortalNameLabel = null;
			}

			if (PortalUrlLabel != null) {
				PortalUrlLabel.Dispose ();
				PortalUrlLabel = null;
			}

			if (UpButton != null) {
				UpButton.Dispose ();
				UpButton = null;
			}

			if (DownButton != null) {
				DownButton.Dispose ();
				DownButton = null;
			}
		}
	}
}
