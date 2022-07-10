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
	[Register ("FilesView")]
	partial class FilesView
	{
		[Outlet]
		UIKit.UIView ActivityIndicatorContainer { get; set; }

		[Outlet]
		UIKit.UIView FileExplorerHolder { get; set; }

		[Outlet]
		UIKit.UILabel LoadingLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (LoadingLabel != null) {
				LoadingLabel.Dispose ();
				LoadingLabel = null;
			}

			if (ActivityIndicatorContainer != null) {
				ActivityIndicatorContainer.Dispose ();
				ActivityIndicatorContainer = null;
			}

			if (FileExplorerHolder != null) {
				FileExplorerHolder.Dispose ();
				FileExplorerHolder = null;
			}
		}
	}
}
