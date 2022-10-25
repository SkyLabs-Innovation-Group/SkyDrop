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
		UIKit.UIActivityIndicatorView ActivityIndicator { get; set; }

		[Outlet]
		UIKit.UIView ActivityIndicatorContainer { get; set; }

		[Outlet]
		UIKit.UILabel ErrorDetailsLabel { get; set; }

		[Outlet]
		UIKit.UIImageView ErrorIcon { get; set; }

		[Outlet]
		UIKit.UIView ExtractArchiveButton { get; set; }

		[Outlet]
		UIKit.UIImageView ExtractArchiveIcon { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView ExtractArchiveSpinner { get; set; }

		[Outlet]
		UIKit.UIView FileExplorerHolder { get; set; }

		[Outlet]
		UIKit.UITableView FoldersTableView { get; set; }

		[Outlet]
		UIKit.UILabel LoadingLabel { get; set; }

		[Outlet]
		UIKit.UIView SaveArchiveButton { get; set; }

		[Outlet]
		UIKit.UIImageView SaveArchiveIcon { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView SaveArchiveSpinner { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (SaveArchiveIcon != null) {
				SaveArchiveIcon.Dispose ();
				SaveArchiveIcon = null;
			}

			if (ExtractArchiveIcon != null) {
				ExtractArchiveIcon.Dispose ();
				ExtractArchiveIcon = null;
			}

			if (SaveArchiveSpinner != null) {
				SaveArchiveSpinner.Dispose ();
				SaveArchiveSpinner = null;
			}

			if (ExtractArchiveSpinner != null) {
				ExtractArchiveSpinner.Dispose ();
				ExtractArchiveSpinner = null;
			}

			if (ActivityIndicator != null) {
				ActivityIndicator.Dispose ();
				ActivityIndicator = null;
			}

			if (ActivityIndicatorContainer != null) {
				ActivityIndicatorContainer.Dispose ();
				ActivityIndicatorContainer = null;
			}

			if (ErrorDetailsLabel != null) {
				ErrorDetailsLabel.Dispose ();
				ErrorDetailsLabel = null;
			}

			if (ErrorIcon != null) {
				ErrorIcon.Dispose ();
				ErrorIcon = null;
			}

			if (ExtractArchiveButton != null) {
				ExtractArchiveButton.Dispose ();
				ExtractArchiveButton = null;
			}

			if (FileExplorerHolder != null) {
				FileExplorerHolder.Dispose ();
				FileExplorerHolder = null;
			}

			if (FoldersTableView != null) {
				FoldersTableView.Dispose ();
				FoldersTableView = null;
			}

			if (LoadingLabel != null) {
				LoadingLabel.Dispose ();
				LoadingLabel = null;
			}

			if (SaveArchiveButton != null) {
				SaveArchiveButton.Dispose ();
				SaveArchiveButton = null;
			}
		}
	}
}
