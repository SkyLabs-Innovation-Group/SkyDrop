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

        public void ReorderPortals(int position, int newPosition)
        {
            Log.Trace($"ReorderPortals(position: {position}, newPosition: {newPosition})");

            if (position == newPosition)
                return;

            var copy = new MvxObservableCollection<SkynetPortalDVM>(UserPortals);
            var portalCopy = copy[position];
            copy.RemoveAt(position);

            int newIndex = Math.Max(0, Math.Min(UserPortals.Count - 1, newPosition));

            copy.Insert(newIndex, portalCopy);

            UserPortals.SwitchTo(copy);
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

