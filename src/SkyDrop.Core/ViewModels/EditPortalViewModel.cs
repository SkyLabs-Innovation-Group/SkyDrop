using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Services;
using Xamarin.Essentials;
using static SkyDrop.Core.ViewModels.EditPortalViewModel;

namespace SkyDrop.Core.ViewModels
{
    public class EditPortalViewModel : BaseViewModel<NavParam>
    {
        public SkynetPortal Portal;

        public string PortalName { get; set; }

        public string PortalUrl { get; set; }

        public string ApiToken { get; set; }

        public IMvxCommand SavePortalCommand { get; set; }

        public EditPortalViewModel(ISingletonService singletonService) : base(singletonService)
        {
            SavePortalCommand = new MvxCommand(SavePortal);
        }

        private void SavePortal()
        {
            Portal ??= new SkynetPortal(PortalUrl, PortalName);

            var portalDVM = new SkynetPortalDVM(Portal)
            {
                Name = PortalName,
                BaseUrl = PortalUrl,
            };

            if (AddingNewPortal)
                singletonService.StorageService.SaveSkynetPortal(Portal, ApiToken);
            else
                singletonService.StorageService.EditSkynetPortal(portalDVM, ApiToken);

            singletonService.UserDialogs.Toast("Saved");
        }

        public class NavParam
        {
            public string PortalId { get; set; }
        }

        public bool AddingNewPortal { get; set; }

        public TaskCompletionSource<bool> PrepareTcs { get; set; } = new TaskCompletionSource<bool>();

        public override async void Prepare(NavParam parameter)
        {
            if (string.IsNullOrEmpty(parameter.PortalId))
            {
                AddingNewPortal = true;
            }
            else
            {
                Portal = singletonService.StorageService.LoadSkynetPortal(parameter.PortalId);
                ApiToken = await SecureStorage.GetAsync(Portal.GetApiTokenPrefKey());
            }

            PrepareTcs.TrySetResult(true);
        }

        public async override void ViewCreated()
        {
            base.ViewCreated();

            await PrepareTcs.Task;

            if (Portal != null)
            {
                PortalName = Portal.Name;
                PortalUrl = Portal.BaseUrl;
            }
        }
    }
}

