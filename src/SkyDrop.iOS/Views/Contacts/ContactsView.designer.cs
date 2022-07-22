// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SkyDrop.iOS.Views.Certificates
{
	[Register ("ContactsView")]
	partial class ContactsView
	{
		[Outlet]
		UIKit.UITableView ContactsTableView { get; set; }

		[Outlet]
		UIKit.UIView ErrorView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ContactsTableView != null) {
				ContactsTableView.Dispose ();
				ContactsTableView = null;
			}

			if (ErrorView != null) {
				ErrorView.Dispose ();
				ErrorView = null;
			}
		}
	}
}
