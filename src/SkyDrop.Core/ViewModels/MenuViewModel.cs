using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.ViewModels.Main
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly IApiService apiService;
        private readonly IBarcodeService barcodeService;
        private readonly IMvxNavigationService navigationService;
        private readonly IStorageService storageService;
        private readonly IUserDialogs userDialogs;

        public MenuViewModel(ISingletonService singletonService,
            IApiService apiService,
            IStorageService storageService,
            IBarcodeService barcodeService,
            IUserDialogs userDialogs,
            IMvxNavigationService navigationService,
            ILog log) : base(singletonService)
        {
            Title = "SkyDrop";

            this.apiService = apiService;
            this.storageService = storageService;
            this.userDialogs = userDialogs;
            this.navigationService = navigationService;
            this.barcodeService = barcodeService;

            NavToDropCommand = new MvxAsyncCommand(async () => await NavToDrop());
            NavToFilesCommand = new MvxAsyncCommand(async () => await NavToFiles());
            NavToBarcodeCommand = new MvxAsyncCommand(async () => await NavToBarcode());
        }

        public IMvxCommand NavToDropCommand { get; set; }
        public IMvxCommand NavToFilesCommand { get; set; }
        public IMvxCommand NavToBarcodeCommand { get; set; }

        private Task NavToDrop()
        {
            return navigationService.Navigate<DropViewModel>();
        }

        private Task NavToFiles()
        {
            return navigationService.Navigate<FilesViewModel>();
        }

        private Task NavToBarcode()
        {
            return navigationService.Navigate<BarcodeViewModel>();
        }
    }
}