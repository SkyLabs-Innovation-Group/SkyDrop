using System;
using System.Collections.Generic;
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

        public MvxObservableCollection<SkynetPortalDVM> UserPortals { get; } = new MvxObservableCollection<SkynetPortalDVM>();

        public static MvxObservableCollection<SkynetPortalDVM> PresetPortals => new MvxObservableCollection<SkynetPortalDVM>(
            new List<SkynetPortalDVM>()
        {
            new SkynetPortalDVM("https://siasky.net", "Siasky.net"),
            new SkynetPortalDVM("https://skynetpro.net", "SkynetPro.net"),
            new SkynetPortalDVM("https://skynetfree.net", "SkynetFree.net"),
            new SkynetPortalDVM("https://siasky.dev", "Siasky.Dev"),
        });

        public PortalPreferencesViewModel(ISingletonService singletonService,
                                 IApiService apiService,
                                 IMvxNavigationService navigationService) : base(singletonService)
        {
            this.apiService = apiService;
            this.navigationService = navigationService;

            Title = "Portal Preferences";

            BackCommand = new MvxAsyncCommand(async () => await navigationService.Close(this));
        }

        public override Task Initialize()
        {
            LoadUserPortals();

            return base.Initialize();
        }

        public void LoadUserPortals()
        {
            UserPortals.SwitchTo(PresetPortals);
        }
    }
}

