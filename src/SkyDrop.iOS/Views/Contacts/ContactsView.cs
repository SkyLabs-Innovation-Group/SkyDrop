using Acr.UserDialogs;
using CoreGraphics;
using MvvmCross.Commands;
using MvvmCross.Platforms.Ios.Binding.Views;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using SkyDrop.iOS.Views.Contacts;
using UIKit;

namespace SkyDrop.iOS.Views.Certificates
{
    [MvxChildPresentation]
    public partial class ContactsView : BaseViewController<ContactsViewModel>
    {
        public ContactsView() : base("ContactsView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ViewModel.CloseKeyboardCommand = new MvxCommand(() => View.EndEditing(true));

            AddBackButton(() => ViewModel.Close());

            PreferredContentSize = new CGSize(300, 300);

            ContactsTableView.BackgroundColor = Colors.DarkGrey.ToNative();

            var pairButton = new UIBarButtonItem { Image = UIImage.FromBundle("ic_pair") };
            pairButton.Clicked += (s, e) => ViewModel.SharePublicKeyCommand.Execute();
            NavigationItem.RightBarButtonItems = new[] { pairButton };

            var source = new MvxSimpleTableViewSource(ContactsTableView, ContactCell.Key);
            ContactsTableView.Source = source;
            ContactsTableView.AllowsSelection = false;
            ContactsTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

            var set = CreateBindingSet();
            set.Bind(source).For(s => s.ItemsSource).To(vm => vm.Contacts);
            set.Bind(ErrorView).For("Visible").To(vm => vm.IsNoContacts);
            set.Bind(this).For(s => s.Title).To(vm => vm.Title);
            set.Apply();
        }
    }
}