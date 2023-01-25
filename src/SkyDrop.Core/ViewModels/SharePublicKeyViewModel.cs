using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using SkyDrop.Core.Services;
using SkyDrop.Core.Utility;
using Xamarin.Essentials;
using ZXing.Common;
using static SkyDrop.Core.Services.EncryptionService;

namespace SkyDrop.Core.ViewModels
{
    public class SharePublicKeyViewModel : BaseViewModel
    {
        private readonly IUserDialogs userDialogs;
        private readonly IBarcodeService barcodeService;
        private readonly IEncryptionService encryptionService;
        private readonly IMvxNavigationService navigationService;
        private bool isBusy;

        private Guid justScannedId;

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

            this.userDialogs = userDialogs;
            this.barcodeService = barcodeService;
            this.encryptionService = encryptionService;
            this.navigationService = navigationService;

            BackCommand = new MvxCommand(() => navigationService.Close(this));
            PasteApiKeyCommand = new MvxAsyncCommand(PasteApiKey);
            ShareApiKeyCommand = new MvxAsyncCommand(ShareApiKey);
        }

        public AddContactResult AddContactResult { get; set; }
        public IMvxCommand BackCommand { get; set; }
        public IMvxCommand RefreshBarcodeCommand { get; set; }
        public IMvxCommand StopScanningCommand { get; set; }
        public IMvxCommand PasteApiKeyCommand { get; set; }
        public IMvxCommand ShareApiKeyCommand { get; set; }
        public string HintText => GetHintText(AddContactResult);
        public string ContactSavedName { get; set; }

        public BitMatrix GenerateBarcode(int width, int height)
        {
            var publicKey = encryptionService.GetMyPublicKeyWithId(justScannedId);
            return barcodeService.GenerateBarcode(publicKey, width, height);
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            if (!Preferences.Get(PreferenceKey.ContactsOnboardingComplete, false))
            {
                userDialogs.Alert("To save a new contact, scan the contact's QR code or share the public key using the buttons in the top right to copy and paste.");
                Preferences.Set(PreferenceKey.ContactsOnboardingComplete, true);
            }
        }

        public void AddContact(string barcodeData, bool isFromClipboard = false)
        {
            try
            {
                if (barcodeData == null)
                {
                    Log.Trace("barcodeData is null");
                    return;
                }

                //wait for user interaction before scanning again
                if (AddContactResult != AddContactResult.Default && !isFromClipboard)
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

        public string GetHintText(AddContactResult result)
        {
            return result switch
            {
                AddContactResult.DevicesPaired => "Devices paired",
                AddContactResult.AlreadyExists => $"Contact already saved as {ContactSavedName}",
                AddContactResult.ContactAdded => "Pairing is complete when both devices show this icon",
                AddContactResult.InvalidKey => "Invalid key",
                AddContactResult.WrongDevice => "Unexpected device! Please go back and try to pair again",
                AddContactResult.Default => "",
                _ => throw new Exception("Unexpected AddContactResult")
            };
        }

        private async Task PasteApiKey()
        {
            var text = await Xamarin.Essentials.Clipboard.GetTextAsync();
            if (text.IsNullOrWhiteSpace())
            {
                AddContactResult = AddContactResult.InvalidKey;
                return;
            }

            AddContact(text.Trim(), true);
        }

        private async Task ShareApiKey()
        {
            var publicKey = encryptionService.GetMyPublicKeyWithId(justScannedId);
            await Xamarin.Essentials.Share.RequestAsync(publicKey);
        }

        public void Close()
        {
            navigationService.Close(this);
        }
    }
}