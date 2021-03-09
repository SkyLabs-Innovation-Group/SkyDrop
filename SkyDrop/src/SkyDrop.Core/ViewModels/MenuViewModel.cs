﻿using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.ViewModels.Main
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly IApiService apiService;
        private readonly IStorageService storageService;
        private readonly IUserDialogs userDialogs;
        private readonly IMvxNavigationService navigationService;
        private readonly IBarcodeService barcodeService;

        public IMvxCommand NavToDropCommand { get; set; }
        public IMvxCommand NavToFilesCommand { get; set; }
        public IMvxCommand NavToBarcodeCommand { get; set; }

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

            NavToDropCommand = new MvxAsyncCommand(NavToDrop);
            NavToFilesCommand = new MvxAsyncCommand(NavToFiles);
            NavToBarcodeCommand = new MvxAsyncCommand(NavToBarcode);
        }

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
