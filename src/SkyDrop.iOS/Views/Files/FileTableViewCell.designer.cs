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
		
		void ReleaseDesignerOutlets ()
		{
			if (FileNameLabel != null) {
				FileNameLabel.Dispose ();
				FileNameLabel = null;
			}
		}
	}
}
