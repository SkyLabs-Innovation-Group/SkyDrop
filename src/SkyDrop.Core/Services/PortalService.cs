using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.ViewModels;

namespace SkyDrop.Core.Services
{
    public class PortalService : IPortalService
    {
        private const string UserPortalsListPrefKey = "userPortalsList";
        private readonly IFileSystemService fileSystemService;
        private readonly ILog log;

        private readonly IMvxNavigationService navigationService;

        private readonly ISingletonService singletonService;

        private readonly IUserDialogs userDialogs;

        public List<SkynetPortal> SkynetPortals;

        public PortalService(ISingletonService singletonService, ILog log,
            IUserDialogs userDialogs, IFileSystemService fileSystemService, IMvxNavigationService navigationService)
        {
            this.singletonService = singletonService;
            this.log = log;
            this.userDialogs = userDialogs;
            this.fileSystemService = fileSystemService;
            this.navigationService = navigationService;
        }

        public List<SkynetPortalDvm> ConvertSkynetPortalsToDvMs(List<SkynetPortal> skynetPortals, Action<SkynetPortal, bool> reorderAction)
        {
            var dvmList = new List<SkynetPortalDvm>();
            foreach (var portal in skynetPortals)
                dvmList.Add(new SkynetPortalDvm(portal)
                {
                    TapCommand = new MvxAsyncCommand(async () => await NavigateToEditPortal(portal.Id)),
                    MoveUpCommand = new MvxCommand(() => reorderAction?.Invoke(portal, true)),
                    MoveDownCommand = new MvxCommand(() => reorderAction?.Invoke(portal, false))
                });
            return dvmList.OrderByDescending(p => p.PortalPreferencesPosition).ToList();
        }

        private Task NavigateToEditPortal(string realmId)
        {
            return navigationService.Navigate<EditPortalViewModel, EditPortalViewModel.NavParam>(
                new EditPortalViewModel.NavParam { PortalId = realmId });
        }
    }

    public interface IPortalService
    {
        List<SkynetPortalDvm> ConvertSkynetPortalsToDvMs(List<SkynetPortal> skynetPortals, Action<SkynetPortal, bool> reorderAction);
    }
}