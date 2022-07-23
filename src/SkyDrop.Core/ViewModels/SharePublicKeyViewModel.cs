using System;
using Acr.UserDialogs;
using MvvmCross.Navigation;
using SkyDrop.Core.Services;
using ZXing.Common;

namespace SkyDrop.Core.ViewModels
{
    public class SharePublicKeyViewModel : BaseViewModel
    {
        private readonly IBarcodeService barcodeService;
        private readonly IEncryptionService encryptionService;
        private readonly IMvxNavigationService navigationService;

        public SharePublicKeyViewModel(ISingletonService singletonService,
            IApiService apiService,
            IStorageService storageService,
            IBarcodeService barcodeService,
            IShareLinkService shareLinkService,
            IUploadTimerService uploadTimerService,
            IUserDialogs userDialogs,
            IMvxNavigationService navigationService,
            IFileSystemService fileSystemService,
            IEncryptionService encryptionService,
            ILog log) : base(singletonService)
        {
            Title = "Public Key";

            this.barcodeService = barcodeService;
            this.encryptionService = encryptionService;
            this.navigationService = navigationService;
        }

        public BitMatrix GenerateBarcode(int width, int height)
        {
            string publicKey = encryptionService.GetMyPublicKeyWithId();
            return barcodeService.GenerateBarcode(publicKey, width, height);
        }

        public void Close()
        {
            navigationService.Close(this);
        }
    }
}

