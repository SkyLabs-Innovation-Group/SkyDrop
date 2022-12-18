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
		UIKit.UISwitch EnableUploadNotificationsSwitch { get; set; }

		[Outlet]
		UIKit.UITextView NameTextView { get; set; }

		[Outlet]
		UIKit.UIButton OnboardingButton { get; set; }

		[Outlet]
		UIKit.UIButton SetNameButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (EnableUploadNotificationsSwitch != null) {
				EnableUploadNotificationsSwitch.Dispose ();
				EnableUploadNotificationsSwitch = null;
			}

			if (NameTextView != null) {
				NameTextView.Dispose ();
				NameTextView = null;
			}

			if (OnboardingButton != null) {
				OnboardingButton.Dispose ();
				OnboardingButton = null;
			}

			if (SetNameButton != null) {
				SetNameButton.Dispose ();
				SetNameButton = null;
			}
		}
	}
}
