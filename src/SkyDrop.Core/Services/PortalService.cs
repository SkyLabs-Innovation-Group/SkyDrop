using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using Realms;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.ViewModels;

namespace SkyDrop.Core.Services
{
  public class PortalService : IPortalService
  {
    private const string UserPortalsListPrefKey = "userPortalsList";
    private readonly ILog log;
    private readonly IUserDialogs userDialogs;
    private readonly IFileSystemService fileSystemService;

    public List<SkynetPortal> skynetPortals;
    private readonly ISingletonService singletonService
      ;

    private readonly IMvxNavigationService navigationService;

    public PortalService(ISingletonService singletonService, ILog log, 
      IUserDialogs userDialogs, IFileSystemService fileSystemService, IMvxNavigationService navigationService)
    {
      this.singletonService = singletonService;
      this.log = log;
      this.userDialogs = userDialogs;
      this.fileSystemService = fileSystemService;
      this.navigationService = navigationService;
    }

    public List<SkynetPortalDVM> ConvertSkynetPortalsToDVMs(List<SkynetPortal> skynetPortals)
    {
      var dvmList = new List<SkynetPortalDVM>();
      foreach (var portal in skynetPortals)
      {
        dvmList.Add(new SkynetPortalDVM(portal)
        {
          TapCommand = new MvxAsyncCommand(async () => await NavigateToEditPortal(portal.Id))
        });
      }
      return dvmList.OrderByDescending(p => p.PortalPreferencesPosition).ToList();
    }

    private Task NavigateToEditPortal(string realmId)
    {
      return navigationService.Navigate<EditPortalViewModel, EditPortalViewModel.NavParam>(
        new EditPortalViewModel.NavParam() { PortalId = realmId });
    }
  }

  public interface IPortalService
  {
    //List<SkynetPortal> GetSavedPortals();
    //void SavePortal(SkynetPortal skynetPortal);
    List<SkynetPortalDVM> ConvertSkynetPortalsToDVMs(List<SkynetPortal> skynetPortals);
  }
}

