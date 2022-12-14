using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Services;
using Xamarin.Essentials;
using static SkyDrop.Core.ViewModels.EditPortalViewModel;

namespace SkyDrop.Core.ViewModels
{
    public class EditPortalViewModel : BaseViewModel<NavParam>
    {
        public SkynetPortal Portal;
        public TaskCompletionSource<bool> PrepareTcs { get; set; } = new TaskCompletionSource<bool>();
        public string PortalName { get; set; }
        public string PortalUrl { get; set; }
        public string ApiToken { get; set; }
        public bool AddingNewPortal { get; set; }
        public IMvxCommand SavePortalCommand { get; set; }
        public IMvxCommand LoginWithPortalCommand { get; set; }
        public IMvxCommand PasteApiKeyCommand { get; set; }
        public IMvxCommand BackCommand { get; set; }

        private readonly IMvxNavigationService navigationService;

        public class NavParam
        {
            public string PortalId { get; set; }
        }

        public EditPortalViewModel(ISingletonService singletonService, IMvxNavigationService navigationService) : base(
            singletonService)
        {
            Title = "Edit Portal";

            BackCommand = new MvxCommand(() => navigationService.Close(this));
            SavePortalCommand = new MvxCommand(SavePortal);
            LoginWithPortalCommand = new MvxAsyncCommand(LoginWithPortal);
            PasteApiKeyCommand = new MvxAsyncCommand(PasteApiKey);
            this.navigationService = navigationService;
        }

        private void SavePortal()
        {
            Portal ??= new SkynetPortal(PortalUrl, PortalName);

            var portalDvm = new SkynetPortalDvm(Portal)
            {
                Name = PortalName,
                BaseUrl = PortalUrl
            };

            if (AddingNewPortal)
                SingletonService.StorageService.SaveSkynetPortal(Portal, ApiToken);
            else
                SingletonService.StorageService.EditSkynetPortal(portalDvm, ApiToken);

            SingletonService.UserDialogs.Toast("Saved");

            navigationService.Close(this);
        }

        public override async void Prepare(NavParam parameter)
        {
            if (string.IsNullOrEmpty(parameter.PortalId))
            {
                AddingNewPortal = true;
            }
            else
            {
                Portal = SingletonService.StorageService.LoadSkynetPortal(parameter.PortalId);
                ApiToken = await SecureStorage.GetAsync(Portal.GetApiTokenPrefKey());
            }

            PrepareTcs.TrySetResult(true);
        }

        public override async void ViewCreated()
        {
            base.ViewCreated();

            await PrepareTcs.Task;

            if (Portal != null)
            {
                PortalName = Portal.Name;
                PortalUrl = Portal.BaseUrl;
            }
        }

        private async Task LoginWithPortal()
        {
            var result = await navigationService.Navigate<PortalLoginViewModel, string, string>(PortalUrl);
            if (!string.IsNullOrEmpty(result))
                ApiToken = result;
        }

        private async Task PasteApiKey()
        {
            var maxLength = 100;
            var text = await Xamarin.Essentials.Clipboard.GetTextAsync();
            if (text == null)
                return;

            text = text.Trim();
            text = text.Substring(0, Math.Min(text.Length, maxLength));
            ApiToken = text;
        }
    }
}