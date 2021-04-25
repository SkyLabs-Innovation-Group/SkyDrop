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
	[Register ("FilePreviewCollectionViewCell")]
	partial class FilePreviewCollectionViewCell
	{
		[Outlet]
		UIKit.UILabel FilenameLabel { get; set; }

		[Outlet]
		UIKit.UIView ImageContainer { get; set; }

		[Outlet]
		UIKit.UIImageView PreviewImage { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (FilenameLabel != null) {
				FilenameLabel.Dispose ();
				FilenameLabel = null;
			}

			if (ImageContainer != null) {
				ImageContainer.Dispose ();
				ImageContainer = null;
			}

			if (PreviewImage != null) {
				PreviewImage.Dispose ();
				PreviewImage = null;
			}
		}
	}
}
