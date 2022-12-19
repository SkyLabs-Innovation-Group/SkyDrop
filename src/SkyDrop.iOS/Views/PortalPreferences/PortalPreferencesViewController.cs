using Acr.UserDialogs;
using MvvmCross.Platforms.Ios.Binding.Views;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using UIKit;

namespace SkyDrop.iOS.Views.PortalPreferences
{
    public partial class PortalPreferencesViewController : BaseViewController<PortalPreferencesViewModel>
    {
        public PortalPreferencesViewController() : base("PortalPreferencesViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var plusButton = new UIBarButtonItem { Image = UIImage.FromBundle("ic_plus") };
            plusButton.Clicked += (s, e) => ViewModel.AddNewPortalCommand?.Execute();
            NavigationItem.RightBarButtonItem = plusButton;

            var portalsTableSource =
                new MvxSimpleTableViewSource(PortalPreferencesTableView, PortalPreferencesCell.Key);
            PortalPreferencesTableView.Source = portalsTableSource;
            PortalPreferencesTableView.RegisterNibForCellReuse(PortalPreferencesCell.Nib, PortalPreferencesCell.Key);
            PortalPreferencesTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            PortalPreferencesTableView.BackgroundColor = Colors.DarkGrey.ToNative();

            var set = CreateBindingSet();
            set.Bind(portalsTableSource).For(f => f.ItemsSource).To(vm => vm.UserPortals);
            set.Bind(this).For(f => f.Title).To(vm => vm.Title);
            set.Apply();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}