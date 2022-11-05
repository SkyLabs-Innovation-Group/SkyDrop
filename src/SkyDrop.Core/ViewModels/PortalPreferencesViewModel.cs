using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.ViewModels
{
	public class PortalPreferencesViewModel : BaseViewModel
	{
        private readonly IApiService apiService;
        private readonly IMvxNavigationService navigationService;

        public IMvxCommand BackCommand { get; set; }

    private readonly IPortalService portalService;

    public MvxObservableCollection<SkynetPortalDVM> UserPortals { get; } = new MvxObservableCollection<SkynetPortalDVM>();

        public PortalPreferencesViewModel(ISingletonService singletonService,
                                          IApiService apiService,
                                          IMvxNavigationService navigationService,
                                          IPortalService portalService) : base(singletonService)
        {
            this.apiService = apiService;
            this.navigationService = navigationService;

            Title = "Portal Preferences";
            UserPortals = new MvxObservableCollection<SkynetPortalDVM>();
            BackCommand = new MvxAsyncCommand(async () => await navigationService.Close(this));
            this.portalService = portalService;
        }

        public async override Task Initialize()
        {
             LoadUserPortals();
        }

        public void LoadUserPortals()
        {
            var savedPortals = portalService.GetSavedPortals();
            var savedPortalsDvms = portalService.ConvertSkynetPortalsToDVMs(savedPortals);
            UserPortals.SwitchTo(savedPortalsDvms);
        }

        public void ReorderPortals(int position, int newPosition)
        {
            if (position == newPosition)
                return;

            var copy = new MvxObservableCollection<SkynetPortalDVM>(UserPortals);
            var portalCopy = copy[position];

            int newIndex = Math.Max(0, Math.Min(UserPortals.Count - 1, newPosition));
            copy.Move(position, newIndex);

            foreach (var portal in copy)
            {
                Log.Trace(portal.Name);
            }

            UserPortals.SwitchTo(copy);

           // TODO save to shared storage
        }
    }
}

