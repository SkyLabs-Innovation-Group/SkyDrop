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
            // Perform any additional setup after loading the view, typically from a nib.

            var portalsTableSource =
                new MvxSimpleTableViewSource(PortalPreferencesTableView, PortalPreferencesCell.Key);
            PortalPreferencesTableView.Source = portalsTableSource;
            PortalPreferencesTableView.RegisterNibForCellReuse(PortalPreferencesCell.Nib, PortalPreferencesCell.Key);
            PortalPreferencesTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            PortalPreferencesTableView.BackgroundColor = Colors.DarkGrey.ToNative();

            var set = CreateBindingSet();
            set.Bind(portalsTableSource).For(f => f.ItemsSource).To(vm => vm.UserPortals);
            set.Apply();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}