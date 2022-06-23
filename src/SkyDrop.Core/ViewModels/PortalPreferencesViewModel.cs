using System;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.ViewModels
{
	public class PortalPreferencesViewModel : BaseViewModel
	{
        private readonly IApiService apiService;
        private readonly IMvxNavigationService navigationService;

        public IMvxCommand BackCommand { get; set; }

        public PortalPreferencesViewModel(ISingletonService singletonService,
                                 IApiService apiService,
                                 IMvxNavigationService navigationService) : base(singletonService)
        {
            this.apiService = apiService;
            this.navigationService = navigationService;

            Title = "Portal Preferences";

            BackCommand = new MvxAsyncCommand(async () => await navigationService.Close(this));
        }
    }
}

