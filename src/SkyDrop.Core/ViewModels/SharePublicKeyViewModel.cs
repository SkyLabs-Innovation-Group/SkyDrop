using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using SkyDrop.Core.Services;
using ZXing.Common;
using static SkyDrop.Core.Services.EncryptionService;

namespace SkyDrop.Core.ViewModels
{
    public class SharePublicKeyViewModel : BaseViewModel
    {
        private readonly IBarcodeService barcodeService;
        private readonly IEncryptionService encryptionService;
        private readonly IMvxNavigationService navigationService;

        public AddContactResult AddContactResult { get; set; }
        public IMvxCommand BackCommand { get; set; }
        public IMvxCommand ScanAgainCommand { get; set; }

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

            BackCommand = new MvxCommand(() => navigationService.Close(this));
        }

        public BitMatrix GenerateBarcode(int width, int height)
        {
            string publicKey = encryptionService.GetMyPublicKeyWithId();
            return barcodeService.GenerateBarcode(publicKey, width, height);
        }

        public async Task AddContact(string barcodeData)
        {
            try
            {
                if (barcodeData == null)
                {
                    Log.Trace("barcodeData is null");
                    return;
                }

                if (AddContactResult != AddContactResult.Default)
                    return;

                //solves issue with disappearing name entry dialog on Android
                //await Task.Delay(500);

                AddContactResult = await encryptionService.AddPublicKey(barcodeData);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public void Close()
        {
            navigationService.Close(this);
        }
    }
}

