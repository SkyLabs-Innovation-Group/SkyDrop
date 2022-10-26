using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Services;
using Xamarin.Essentials;
using static SkyDrop.Core.ViewModels.OnboardingViewModel;

namespace SkyDrop.Core.ViewModels
{
    public class OnboardingViewModel : BaseViewModel
    {
        public string OnboardingText { get; set; }
        public IMvxCommand BackCommand { get; set; }

        private IMvxNavigationService navigationService;

        public OnboardingViewModel(ISingletonService singletonService,
            IMvxNavigationService navigationService) : base(singletonService)
        {
            this.navigationService = navigationService;

            Title = "SkyDrop";

            BackCommand = new MvxCommand(Dismiss);
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            OnboardingText = "New in SkyDrop V2\n\nSkyDrive\n- Recently sent and received files are saved in the SkyDrive for easy access\n- Create your own folders to organise your files\n\nPortals\n- Choose your preferred Skynet portals and rank them in order of preference\n- Uploads will first be attempted on your first choice portal, if the upload fails then the upload will be retried using the next portal in the list\n\nEnd-to-end encryption\n- Send encrypted files to your contacts for additional data security\n- To add a new contact, pair with the contact's device by scanning their public key QR code\n- Encrypted files can only be decrypted by the specified recipient device";
        }

        private void Dismiss()
        {
            Preferences.Set(PreferenceKey.OnboardingComplete, true);
            navigationService.Close(this);
        }
    }
}

