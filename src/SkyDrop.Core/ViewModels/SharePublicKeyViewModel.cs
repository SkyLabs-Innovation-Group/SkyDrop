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
        public IMvxCommand RefreshBarcodeCommand { get; set; }

        private Guid justScannedId = default;
        private bool isBusy;

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
            string publicKey = encryptionService.GetMyPublicKeyWithId(justScannedId);
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

                //wait for user interaction before scanning again
                if (AddContactResult != AddContactResult.Default) 
                    return;

                //prevents double dialog issue
                if (isBusy) 
                    return;

                isBusy = true;
                (AddContactResult, justScannedId) = await encryptionService.AddPublicKey(barcodeData);
                isBusy = false;

                RefreshBarcodeCommand.Execute();
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

