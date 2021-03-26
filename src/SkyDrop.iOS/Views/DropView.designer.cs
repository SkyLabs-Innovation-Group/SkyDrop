// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SkyDrop.iOS.Views.Drop
{
	[Register ("DropView")]
	partial class DropView
	{
		[Outlet]
		UIKit.UIButton DropButton { get; set; }

		[Outlet]
		UIKit.UIImageView ReceiveButton { get; set; }

		[Outlet]
		UIKit.UIImageView SendButton { get; set; }

		[Action ("DropViewClickAction:")]
		partial void DropViewClickAction (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (SendButton != null) {
				SendButton.Dispose ();
				SendButton = null;
			}

			if (ReceiveButton != null) {
				ReceiveButton.Dispose ();
				ReceiveButton = null;
			}

			if (DropButton != null) {
				DropButton.Dispose ();
				DropButton = null;
			}
		}
	}
}
