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
		UIKit.UIView CustomViewHolder { get; set; }

		[Outlet]
		UIKit.UICollectionView FilesCollectionView { get; set; }

		[Outlet]
		UIKit.UITableView FilesTableView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (FilesCollectionView != null) {
				FilesCollectionView.Dispose ();
				FilesCollectionView = null;
			}

			if (FilesTableView != null) {
				FilesTableView.Dispose ();
				FilesTableView = null;
			}

			if (CustomViewHolder != null) {
				CustomViewHolder.Dispose ();
				CustomViewHolder = null;
			}
		}
	}
}
