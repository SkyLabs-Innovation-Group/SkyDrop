// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SkyDrop.iOS.Views.Onboarding
{
	[Register ("OnboardingView")]
	partial class OnboardingView
	{
		[Outlet]
		UIKit.UIButton GotItButton { get; set; }

		[Outlet]
		UIKit.UITextView MainTextView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (MainTextView != null) {
				MainTextView.Dispose ();
				MainTextView = null;
			}

			if (GotItButton != null) {
				GotItButton.Dispose ();
				GotItButton = null;
			}
		}
	}
}
