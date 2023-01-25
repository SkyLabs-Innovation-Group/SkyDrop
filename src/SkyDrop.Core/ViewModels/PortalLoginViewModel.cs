using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.ViewModels
{
    public class PortalLoginViewModel : BaseViewModel, IMvxViewModel<string, string>
    {
        private readonly IApiService apiService;
        private readonly IMvxNavigationService navigationService;
        private readonly IStorageService storageService;
        private readonly IUserDialogs userDialogs;

        public PortalLoginViewModel(ISingletonService singletonService,
            IApiService apiService,
            IStorageService storageService,
            IUserDialogs userDialogs,
            IMvxNavigationService navigationService,
            ILog log) : base(singletonService)
        {
            Title = "Login";

            this.apiService = apiService;
            this.storageService = storageService;
            this.userDialogs = userDialogs;
            this.navigationService = navigationService;

            BackCommand = new MvxCommand(() => navigationService.Close(this, null));
        }

        public string LoadingLabelText { get; set; }
        public string PortalUrl { get; set; }
        public bool DidSetApiKey { get; set; }
        public bool IsLoggedIn { get; set; }
        public IMvxCommand BackCommand { get; set; }
        public TaskCompletionSource<object> CloseCompletionSource { get; set; }

        public void Prepare(string url)
        {
            //remove protocol
            if (url.StartsWith("https://"))
                url = url.Substring(8);

            LoadingLabelText = $"Logging in to {url}...";
            Title = url;

            PortalUrl = $"https://account.{url}";
        }

        public void SetApiKey(string apiKey)
        {
            if (DidSetApiKey)
                return;

            DidSetApiKey = true;

            userDialogs.Toast("Logged in");

            navigationService.Close(this, apiKey);
        }
    }
}