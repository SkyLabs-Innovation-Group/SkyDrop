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
	[Register ("PortalPreferencesViewController")]
	partial class PortalPreferencesViewController
	{
		[Outlet]
		UIKit.UITableView PortalPreferencesTableView { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (PortalPreferencesTableView != null) {
				PortalPreferencesTableView.Dispose ();
				PortalPreferencesTableView = null;
			}

		}
	}
}
