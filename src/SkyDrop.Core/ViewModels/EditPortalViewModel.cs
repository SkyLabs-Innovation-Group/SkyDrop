using System;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Services;
using static SkyDrop.Core.ViewModels.EditPortalViewModel;

namespace SkyDrop.Core.ViewModels
{
  public class EditPortalViewModel : BaseViewModel<NavParam, IContactItem>
  {
    public EditPortalViewModel(ISingletonService singletonService) : base(singletonService)
    {
    }

    public class NavParam
    {
      public int PortalId { get; set; }
    }

    public override void Prepare(NavParam parameter)
    {

    }
  }
}

