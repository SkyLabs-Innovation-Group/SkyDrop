using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.ViewModels
{
    public class OnboardingViewModel : BaseViewModel
    {
        public string OnboardingText { get; set; }

        public IMvxCommand BackCommand { get; set; }

        public OnboardingViewModel(ISingletonService singletonService,
            IMvxNavigationService navigationService) : base(singletonService)
        {
            Title = "SkyDrop";

            BackCommand = new MvxCommand(() => navigationService.Close(this));
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            OnboardingText = "Hello there. This is the onboarding text. We do hope you enjoy reading the onboarding text here on SkyDrop.";
        }
    }
}

