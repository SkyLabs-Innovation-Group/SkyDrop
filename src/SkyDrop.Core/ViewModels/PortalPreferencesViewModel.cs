using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Org.BouncyCastle.Utilities;
using Realms;
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

            SetTopPortalSelected();
        }

        private void ReorderAction(SkynetPortal portal, bool moveUp)
        {
            var portalDvm = UserPortals.FirstOrDefault(a => a.Portal == portal);
            var portalCurrentIndex = UserPortals.IndexOf(portalDvm);
            var portalNewIndex = moveUp ? portalCurrentIndex - 1 : portalCurrentIndex + 1;
            
            ReorderPortals(portal, portalCurrentIndex, portalNewIndex);
        }

        private void ReorderPortals(SkynetPortal portal, int oldPosition, int newPosition)
        {
            if (oldPosition == newPosition)
                return;

            if (oldPosition < 0 || oldPosition >= UserPortals.Count || newPosition < 0 || newPosition >= UserPortals.Count)
                return;

            var temp = UserPortals[oldPosition];
            UserPortals[oldPosition] = UserPortals[newPosition];
            UserPortals[newPosition] = temp;

            UserPortals[oldPosition].PortalPreferencesPosition = oldPosition;
            UserPortals[newPosition].PortalPreferencesPosition = newPosition;

            storageService.ReorderPortals(portal, oldPosition, newPosition);
            
            SetTopPortalSelected();
        }

        public void EditPortal(int position)
        {
            var portalId = UserPortals[position].RealmId;
            navigationService.Navigate<EditPortalViewModel, NavParam>(new NavParam { PortalId = portalId });
        }

        private void SetTopPortalSelected()
        {
            var topPortal = UserPortals.FirstOrDefault();
            if (topPortal == null)
                return;

            SkynetPortal.SelectedPortal = topPortal.Portal;
        }
    }
}