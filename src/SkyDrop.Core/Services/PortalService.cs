using System;
using System.Collections.Generic;
using System.Linq;
using Acr.UserDialogs;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;

namespace SkyDrop.Core.Services
{
  public class PortalService : IPortalService
  {
    private const string UserPortalsListPrefKey = "userPortalsList";
    private readonly ILog log;
    private readonly IUserDialogs userDialogs;
    private readonly IFileSystemService fileSystemService;

    public List<SkynetPortal> skynetPortals;

    public PortalService(ILog log, IUserDialogs userDialogs, IFileSystemService fileSystemService)
    {
      this.log = log;
      this.userDialogs = userDialogs;
      this.fileSystemService = fileSystemService;
    }

    public void SavePortal(SkynetPortal skynetPortal)
    {
      var portalNames = Xamarin.Essentials.Preferences.Get(UserPortalsListPrefKey, "");
      var portalNameList = portalNames.Split(',');

      if (portalNameList.Contains(skynetPortal.BaseUrl))
        return;

      portalNames = portalNames + skynetPortal.BaseUrl + ",";
    }

    public List<SkynetPortal> GetSavedPortals()
    {
      var portalList = new List<SkynetPortal>();
      var portalNameList = Xamarin.Essentials.Preferences.Get(UserPortalsListPrefKey, "https://web3portal.com");

      if (portalNameList == null)
        return portalList;


      foreach (string portal in portalNameList.Split(','))
      {
        portalList.Add(new SkynetPortal(portal));
      }

      return portalList;
    }

    public List<SkynetPortalDVM> ConvertSkynetPortalsToDVMs(List<SkynetPortal> skynetPortals)
    {
      var dvmList = new List<SkynetPortalDVM>();
      foreach (var portal in skynetPortals)
      {
        dvmList.Add(new SkynetPortalDVM(portal.BaseUrl, portal.Name));
      }
      return dvmList;
    }
  }

  public interface IPortalService
  {
    List<SkynetPortal> GetSavedPortals();
    void SavePortal(SkynetPortal skynetPortal);
    List<SkynetPortalDVM> ConvertSkynetPortalsToDVMs(List<SkynetPortal> skynetPortals);
  }
}

