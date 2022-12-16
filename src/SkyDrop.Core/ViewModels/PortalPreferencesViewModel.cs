using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Services;
using static SkyDrop.Core.ViewModels.EditPortalViewModel;

namespace SkyDrop.Core.ViewModels
{
    public class PortalPreferencesViewModel : BaseViewModel
    {
        private readonly IApiService apiService;
        private readonly IMvxNavigationService navigationService;

        private readonly IPortalService portalService;
        private readonly IStorageService storageService;

        public PortalPreferencesViewModel(ISingletonService singletonService,
            IApiService apiService,
            IMvxNavigationService navigationService,
            IPortalService portalService,
            IStorageService storageService) : base(singletonService)
        {
            this.apiService = apiService;
            this.navigationService = navigationService;
            this.storageService = storageService;

            Title = "Portal Preferences";
            UserPortals = new MvxObservableCollection<SkynetPortalDvm>();
            BackCommand = new MvxAsyncCommand(async () => await navigationService.Close(this));
            AddNewPortalCommand = new MvxAsyncCommand(async () => await AddNewPortal());
            this.portalService = portalService;
        }

        public IMvxCommand BackCommand { get; set; }

        public IMvxCommand AddNewPortalCommand { get; set; }

        public MvxObservableCollection<SkynetPortalDvm> UserPortals { get; } =
            new MvxObservableCollection<SkynetPortalDvm>();

        private Task AddNewPortal()
        {
            return navigationService.Navigate<EditPortalViewModel, NavParam>(new NavParam());
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();
            LoadUserPortals();
        }

        public void LoadUserPortals()
        {
            var savedPortals = storageService.LoadSkynetPortals();

            if (savedPortals.Count == 0)
            {
                storageService.SaveSkynetPortal(new SkynetPortal(SkynetPortal.DefaultWeb3PortalUrl)
                    { Name = "Web3 Portal" });
                savedPortals = storageService.LoadSkynetPortals();
            }

            var savedPortalsDvms = portalService.ConvertSkynetPortalsToDvMs(savedPortals, ReorderAction);
            UserPortals.SwitchTo(savedPortalsDvms);
        }

        private void ReorderAction(SkynetPortal portal, bool moveUp)
        {
            var portalDvm = UserPortals.FirstOrDefault(a => a.Portal == portal);
            var portalCurrentIndex = UserPortals.IndexOf(portalDvm);
            var portalNewIndex = moveUp ? portalCurrentIndex - 1 : portalCurrentIndex + 1;

            ReorderPortals(portalCurrentIndex, portalNewIndex);
        }

        private void ReorderPortals(int position, int newPosition)
        {
            if (position == newPosition)
                return;

            var copy = new MvxObservableCollection<SkynetPortalDvm>(UserPortals);
            var portalCopy = copy[position];

            var newIndex = Math.Max(0, Math.Min(UserPortals.Count - 1, newPosition));
            copy.Move(position, newIndex);

            foreach (var portal in copy) storageService.EditSkynetPortal(portal.RealmId);

            UserPortals.SwitchTo(copy);
        }

        public void EditPortal(int position)
        {
            var portalId = UserPortals[position].RealmId;
            navigationService.Navigate<EditPortalViewModel, NavParam>(new NavParam { PortalId = portalId });
        }
    }
}