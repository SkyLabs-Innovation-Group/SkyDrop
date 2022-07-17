using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.ViewModels
{
    public class ContactsViewModel : BaseViewModel
    {
        public List<Contact> Contacts { get; set; }
        public IMvxCommand AddContactCommand { get; set; }
        public IMvxCommand SharePublicKeyCommand { get; set; }

        private readonly IApiService apiService;
        private readonly IStorageService storageService;
        private readonly IUserDialogs userDialogs;
        private readonly IMvxNavigationService navigationService;
        private readonly IFileSystemService fileSystemService;
        private readonly IBarcodeService barcodeService;
        private readonly IShareLinkService shareLinkService;
        private readonly IUploadTimerService uploadTimerService;
        private readonly IEncryptionService encryptionService;

        public ContactsViewModel(ISingletonService singletonService,
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
            Log = log;
            Title = "SkyDrop";

            this.apiService = apiService;
            this.storageService = storageService;
            this.userDialogs = userDialogs;
            this.navigationService = navigationService;
            this.fileSystemService = fileSystemService;
            this.barcodeService = barcodeService;
            this.shareLinkService = shareLinkService;
            this.uploadTimerService = uploadTimerService;
            this.encryptionService = encryptionService;

            AddContactCommand = new MvxAsyncCommand(AddContact);
            SharePublicKeyCommand = new MvxAsyncCommand(SharePublicKey);
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            LoadCertificates();
        }

        private void LoadCertificates()
        {
            Contacts = storageService.LoadContacts();
        }

        private async Task AddContact()
        {
            try
            {
                //open the QR code scan view
                var barcodeData = await barcodeService.ScanBarcode();
                if (barcodeData == null)
                {
                    Log.Trace("barcodeData is null");
                    return;
                }

                await encryptionService.AddPublicKey(barcodeData);

                LoadCertificates();
            }
            catch(Exception e)
            {
                Log.Exception(e);
            }
        }

        private Task SharePublicKey()
        {
            return navigationService.Navigate<SharePublicKeyViewModel>();
        }
    }
}

