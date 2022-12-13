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
		UIKit.UIImageView Icon { get; set; }

		[Outlet]
		UIKit.UILabel PortalNameLabel { get; set; }

		[Outlet]
		UIKit.UILabel PortalUrlLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (PortalNameLabel != null) {
				PortalNameLabel.Dispose ();
				PortalNameLabel = null;
			}

			if (PortalUrlLabel != null) {
				PortalUrlLabel.Dispose ();
				PortalUrlLabel = null;
			}

			if (Icon != null) {
				Icon.Dispose ();
				Icon = null;
			}
		}
	}
}
