using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Services;
using Xamarin.Essentials;

namespace SkyDrop.Core.ViewModels
{
    public class OnboardingViewModel : BaseViewModel
    {
        private readonly IMvxNavigationService navigationService;

        private readonly int totalPages;

        public OnboardingViewModel(ISingletonService singletonService,
            IMvxNavigationService navigationService) : base(singletonService)
        {
            this.navigationService = navigationService;

            Title = "What's new?";

            BackCommand = new MvxCommand(Dismiss);

            totalPages = OnboardingContent.Content.Length;
        }

        public string TitleText { get; set; }
        public string DescriptionText { get; set; }
        public string Icon { get; set; }
        public int CurrentPage { get; set; }
        public bool IsLastPage => CurrentPage >= totalPages - 1;
        public bool IsFirstPage => CurrentPage == 0;
        public IMvxCommand BackCommand { get; set; }
        public IMvxCommand NextPageCommand { get; set; }
        public IMvxCommand PreviousPageCommand { get; set; }

        public override async Task Initialize()
        {
            await base.Initialize();

            NextPageCommand = new MvxCommand(NextPage);
            PreviousPageCommand = new MvxCommand(PreviousPage);
            UpdateText();
        }

        private void Dismiss()
        {
            var seenBefore = Preferences.ContainsKey(PreferenceKey.OnboardingComplete);

            Preferences.Set(PreferenceKey.OnboardingComplete, true);
            navigationService.Close(this);

            if (!seenBefore)
                UserDialogs.Instance.Toast("Head to settings to see these screens again");
        }

        private void UpdateText()
        {
            var content = OnboardingContent.Content[CurrentPage];
            TitleText = content.Title;
            DescriptionText = content.Description;
            Icon = content.Icon;
        }

        private void NextPage()
        {
            CurrentPage++;

            if (CurrentPage > totalPages - 1)
            {
                //Done button was tapped
                Dismiss();
                return;
            }

            UpdateText();
        }

        private void PreviousPage()
        {
            CurrentPage--;

            if (CurrentPage < 0)
                CurrentPage = 0;

            UpdateText();
        }
    }
}