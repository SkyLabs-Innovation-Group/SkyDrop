// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SkyDrop.iOS.Views.Portals
{
	[Register ("PortalLoginViewController")]
	partial class PortalLoginViewController
	{
		[Outlet]
		UIKit.UILabel LoggingInLabel { get; set; }

		[Outlet]
		UIKit.UIView WebViewContainer { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (WebViewContainer != null) {
				WebViewContainer.Dispose ();
				WebViewContainer = null;
			}

			if (LoggingInLabel != null) {
				LoggingInLabel.Dispose ();
				LoggingInLabel = null;
			}
		}
	}
}
