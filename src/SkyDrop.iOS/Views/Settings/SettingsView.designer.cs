// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SkyDrop.iOS.Views.Settings
{
	[Register ("SettingsView")]
	partial class SettingsView
	{
		[Outlet]
		UIKit.UIButton ContactsButton { get; set; }

		[Outlet]
		UIKit.UISwitch EnableUploadNotificationsSwitch { get; set; }

		[Outlet]
		UIKit.UITextView NameTextView { get; set; }

		[Outlet]
		UIKit.UITextView PortalTextView { get; set; }

		[Outlet]
		UIKit.UIButton SavePortalButton { get; set; }

		[Outlet]
		UIKit.UIButton SetNameButton { get; set; }

		[Outlet]
		UIKit.UILabel SetPortalLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ContactsButton != null) {
				ContactsButton.Dispose ();
				ContactsButton = null;
			}

			if (EnableUploadNotificationsSwitch != null) {
				EnableUploadNotificationsSwitch.Dispose ();
				EnableUploadNotificationsSwitch = null;
			}

			if (NameTextView != null) {
				NameTextView.Dispose ();
				NameTextView = null;
			}

			if (PortalTextView != null) {
				PortalTextView.Dispose ();
				PortalTextView = null;
			}

			if (SavePortalButton != null) {
				SavePortalButton.Dispose ();
				SavePortalButton = null;
			}

			if (SetNameButton != null) {
				SetNameButton.Dispose ();
				SetNameButton = null;
			}

			if (SetPortalLabel != null) {
				SetPortalLabel.Dispose ();
				SetPortalLabel = null;
			}
		}
	}
}
