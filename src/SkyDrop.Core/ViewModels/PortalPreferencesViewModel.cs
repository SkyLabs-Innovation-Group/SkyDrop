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
using SkyDrop.Core.Utility;
using Xamarin.Essentials;
using static SkyDrop.Core.ViewModels.EditPortalViewModel;

namespace SkyDrop.Core.ViewModels
{
    public class PortalPreferencesViewModel : BaseViewModel
    {
        private readonly IApiService apiService;
        private readonly IMvxNavigationService navigationService;
        private readonly IPortalService portalService;
        private readonly IStorageService storageService;
        private readonly ISkyDropHttpClientFactory httpClientFactory;

        public PortalPreferencesViewModel(ISingletonService singletonService,
            IApiService apiService,
            IMvxNavigationService navigationService,
            IPortalService portalService,
            IStorageService storageService,
            ISkyDropHttpClientFactory httpClientFactory) : base(singletonService)
        {
            this.apiService = apiService;
            this.navigationService = navigationService;
            this.storageService = storageService;
            this.httpClientFactory = httpClientFactory;

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

        public override async void ViewAppearing()
        {
            base.ViewAppearing();
            await LoadUserPortals();
        }

        public async Task LoadUserPortals()
        {
            var savedPortals = await storageService.LoadSkynetPortals();
            if (savedPortals.Count == 0)
            {
                await storageService.SaveSkynetPortal(new SkynetPortal(SkynetPortal.DefaultWeb3PortalUrl, "Web3 Portal"), string.Empty);
                savedPortals = await storageService.LoadSkynetPortals();
            }

            var savedPortalsDvms = portalService.ConvertSkynetPortalsToDvMs(savedPortals, ReorderAction);
            UserPortals.SwitchTo(savedPortalsDvms);

            await SetTopPortalSelected();
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
            
            SetTopPortalSelected().Forget();
        }

        public void EditPortal(int position)
        {
            var portalId = UserPortals[position].RealmId;
            navigationService.Navigate<EditPortalViewModel, NavParam>(new NavParam { PortalId = portalId });
        }

        private async Task SetTopPortalSelected()
        {
            try
            {
                var topPortal = UserPortals.FirstOrDefault();
                if (topPortal == null)
                    return;

                SkynetPortal.SelectedPortal = topPortal.Portal;

                var apiToken = await topPortal.GetApiToken();
                httpClientFactory.UpdateHttpClientWithNewToken(SkynetPortal.SelectedPortal, apiToken);
            }
            catch(Exception e)
            {
                Log.Exception(e);
            }
        }
    }
}