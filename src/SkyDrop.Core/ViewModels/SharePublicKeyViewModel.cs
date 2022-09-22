using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using SkyDrop.Core.Services;
using SkyDrop.Core.Utility;
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
        public IMvxCommand RefreshBarcodeCommand { get; set; }
        public IMvxCommand StopScanningCommand { get; set; }
        public string HintText => GetHintText(AddContactResult);
        public string ContactSavedName { get; set; }

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
            Title = "Pair Devices";

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

        public void AddContact(string barcodeData)
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
                (AddContactResult, justScannedId, ContactSavedName) = encryptionService.AddPublicKey(barcodeData);
                isBusy = false;

                StopScanningCommand.Execute();
                RefreshBarcodeCommand.Execute();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public string GetHintText(AddContactResult result) => result switch
        {
            AddContactResult.DevicesPaired => "Devices paired",
            AddContactResult.AlreadyExists => $"Contact already saved as {ContactSavedName}",
            AddContactResult.ContactAdded => "Pairing is complete when both devices show this icon",
            AddContactResult.InvalidKey => "Invalid key",
            AddContactResult.WrongDevice => "Unexpected device! Please go back and try to pair again",
            AddContactResult.Default => "",
            _ => throw new Exception("Unexpected AddContactResult")
        };

        public void Close()
        {
            navigationService.Close(this);
        }
    }
}

