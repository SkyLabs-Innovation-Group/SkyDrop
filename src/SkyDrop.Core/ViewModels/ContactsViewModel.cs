using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Services;
using static SkyDrop.Core.ViewModels.ContactsViewModel;

namespace SkyDrop.Core.ViewModels
{
    public class ContactsViewModel : BaseViewModel<NavParam, IContactItem>
    {
        public class NavParam { }

        public List<IContactItem> Contacts { get; set; }
        public IMvxCommand SharePublicKeyCommand { get; set; }
        public IMvxCommand BackCommand { get; set; }
        public bool IsNoContacts { get; set; }

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
            Title = "Contacts";

            this.apiService = apiService;
            this.storageService = storageService;
            this.userDialogs = userDialogs;
            this.navigationService = navigationService;
            this.fileSystemService = fileSystemService;
            this.barcodeService = barcodeService;
            this.shareLinkService = shareLinkService;
            this.uploadTimerService = uploadTimerService;
            this.encryptionService = encryptionService;

            SharePublicKeyCommand = new MvxCommand(SharePublicKey);
            BackCommand = new MvxCommand(() => navigationService.Close(this));
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            LoadCertificates();
        }

        private void LoadCertificates()
        {
            var newContacts = storageService.LoadContacts().Select(GetContactDVM).ToList();
            var anyoneWithTheLinkItem = new AnyoneWithTheLinkItem();
            anyoneWithTheLinkItem.TapCommand = new MvxCommand(() => ItemSelected(anyoneWithTheLinkItem));

            if (newContacts == null || newContacts.Count == 0)
                IsNoContacts = true;
            else 
                newContacts.Insert(0, anyoneWithTheLinkItem);

            Contacts = newContacts;

            RaisePropertyChanged(() => Contacts).Forget();
            RaisePropertyChanged(() => IsNoContacts).Forget();
        }

        public IContactItem GetContactDVM(Contact contact)
        {
            var contactItem = new ContactDVM { Contact = contact };
            contactItem.TapCommand = new MvxCommand(() => ItemSelected(contactItem));
            contactItem.DeleteCommand = new MvxAsyncCommand(async () => await DeleteContact(contactItem));
            return contactItem;
        }

        private void SharePublicKey()
        {
            navigationService.Navigate<SharePublicKeyViewModel>();
        }

        private void ItemSelected(IContactItem item)
        {
            navigationService.Close(this, item);
        }

        private async Task DeleteContact(IContactItem item)
        {
            if (item is ContactDVM contactDVM)
            {
                if (!await userDialogs.ConfirmAsync($"Delete contact {contactDVM.Name}?"))
                    return;

                storageService.DeleteContact(contactDVM.Contact);
                LoadCertificates();
            }
        }

        public override void Prepare(NavParam parameter)
        {

        }

        public void Close()
        {
            ItemSelected(null);
        }
    }
}

