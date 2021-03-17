// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SkyDrop.iOS.Views.Menu
{
	[Register ("MenuView")]
	partial class MenuView
	{
		[Outlet]
		UIKit.UIView FileTransferButton { get; set; }

		[Outlet]
		UIKit.UIImageView FileTransferIcon { get; set; }

		[Outlet]
		UIKit.UILabel FileTransferLabel { get; set; }

		[Outlet]
		UIKit.UIView SettingsButton { get; set; }

		[Outlet]
		UIKit.UIImageView SettingsIcon { get; set; }

		[Outlet]
		UIKit.UILabel SettingsLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (FileTransferButton != null) {
				FileTransferButton.Dispose ();
				FileTransferButton = null;
			}

			if (SettingsButton != null) {
				SettingsButton.Dispose ();
				SettingsButton = null;
			}

			if (FileTransferLabel != null) {
				FileTransferLabel.Dispose ();
				FileTransferLabel = null;
			}

			if (SettingsLabel != null) {
				SettingsLabel.Dispose ();
				SettingsLabel = null;
			}

			if (FileTransferIcon != null) {
				FileTransferIcon.Dispose ();
				FileTransferIcon = null;
			}

			if (SettingsIcon != null) {
				SettingsIcon.Dispose ();
				SettingsIcon = null;
			}
		}
	}
}
