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
	[Register ("FileTableViewCell")]
	partial class FileTableViewCell
	{
		[Outlet]
		UIKit.UILabel FileNameLabel { get; set; }

		[Outlet]
		UIKit.UIView SelectedIndicatorInnerView { get; set; }

		[Outlet]
		UIKit.UIView SelectedIndicatorView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (SelectedIndicatorInnerView != null) {
				SelectedIndicatorInnerView.Dispose ();
				SelectedIndicatorInnerView = null;
			}

			if (SelectedIndicatorView != null) {
				SelectedIndicatorView.Dispose ();
				SelectedIndicatorView = null;
			}

			if (FileNameLabel != null) {
				FileNameLabel.Dispose ();
				FileNameLabel = null;
			}
		}
	}
}
