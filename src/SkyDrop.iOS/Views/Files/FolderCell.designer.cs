// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SkyDrop.iOS.Views.Files
{
	[Register ("FolderCell")]
	partial class FolderCell
	{
		[Outlet]
		UIKit.UIView ContainerView { get; set; }

		[Outlet]
		UIKit.UILabel NameLabel { get; set; }

		[Outlet]
		UIKit.UIView SelectedIndicatorInnerView { get; set; }

		[Outlet]
		UIKit.UIView SelectedIndicatorView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ContainerView != null) {
				ContainerView.Dispose ();
				ContainerView = null;
			}

			if (NameLabel != null) {
				NameLabel.Dispose ();
				NameLabel = null;
			}

			if (SelectedIndicatorInnerView != null) {
				SelectedIndicatorInnerView.Dispose ();
				SelectedIndicatorInnerView = null;
			}

			if (SelectedIndicatorView != null) {
				SelectedIndicatorView.Dispose ();
				SelectedIndicatorView = null;
			}
		}
	}
}
