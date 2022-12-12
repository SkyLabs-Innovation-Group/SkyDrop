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
using SkyDrop.Core.Utility;
using static SkyDrop.Core.ViewModels.ContactsViewModel;

namespace SkyDrop.Core.ViewModels
{
    public class ContactsViewModel : BaseViewModel<NavParam, IContactItem>
    {
        private readonly IApiService apiService;
        private readonly IBarcodeService barcodeService;
        private readonly IEncryptionService encryptionService;
        private readonly IFileSystemService fileSystemService;
        private readonly IMvxNavigationService navigationService;
        private readonly IShareLinkService shareLinkService;
        private readonly IStorageService storageService;
        private readonly IUploadTimerService uploadTimerService;
        private readonly IUserDialogs userDialogs;

        private bool isSelecting;

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

        public List<IContactItem> Contacts { get; set; }
        public IMvxCommand SharePublicKeyCommand { get; set; }
        public IMvxCommand BackCommand { get; set; }
        public IMvxCommand CloseKeyboardCommand { get; set; }
        public bool IsNoContacts { get; set; }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            LoadContacts();
        }

        private void LoadContacts()
        {
            var newContacts = storageService.LoadContacts().Select(GetContactDVM).ToList();
            IsNoContacts = newContacts == null || newContacts.Count == 0;
            if (isSelecting && !IsNoContacts)
            {
                var anyoneWithTheLinkItem = new AnyoneWithTheLinkItem();
                anyoneWithTheLinkItem.TapCommand = new MvxCommand(() => ItemSelected(anyoneWithTheLinkItem));
                newContacts.Insert(0, anyoneWithTheLinkItem);
            }

            Contacts = newContacts;

            RaisePropertyChanged(() => Contacts).Forget();
            RaisePropertyChanged(() => IsNoContacts).Forget();
        }

        public IContactItem GetContactDVM(Contact contact)
        {
            var contactItem = new ContactDVM { Contact = contact };
            contactItem.DeleteCommand = new MvxAsyncCommand(async () => await DeleteContact(contactItem));
            contactItem.RenameCommand = new MvxAsyncCommand(async () => await RenameContact(contactItem));
            if (isSelecting)
                contactItem.TapCommand = new MvxCommand(() => ItemSelected(contactItem));
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
            try
            {
                if (item is ContactDVM contactDVM)
                {
                    if (!await userDialogs.ConfirmAsync($"Delete contact {contactDVM.Name}?"))
                        return;

                    storageService.DeleteContact(contactDVM.Contact);
                    LoadContacts();
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
                userDialogs.Toast("Error: Failed to delete contact");
            }
        }

        private async Task RenameContact(IContactItem item)
        {
            try
            {
                if (item is ContactDVM contactDVM)
                {
                    var result = await userDialogs.PromptAsync("Contact name:", null, null, null, contactDVM.Name);
                    if (result.Text.IsNullOrEmpty())
                        return;

                    storageService.RenameContact(contactDVM.Contact, result.Text.Trim());
                    LoadContacts();
                }
            }
            catch (Exception e)
            {
                userDialogs.Toast(e.Message);
            }
            finally
            {
                CloseKeyboardCommand.Execute();
            }
        }

        public override void Prepare(NavParam parameter)
        {
            isSelecting = parameter.IsSelecting;
        }

        public void Close()
        {
            ItemSelected(null);
        }

        public class NavParam
        {
            public bool IsSelecting { get; set; }
        }
    }
}