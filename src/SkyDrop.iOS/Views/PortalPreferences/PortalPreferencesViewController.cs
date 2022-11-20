using System;
using SkyDrop.Core.ViewModels;
using UIKit;

namespace SkyDrop.iOS.Views.PortalPreferences
{
  public partial class PortalPreferencesViewController : BaseViewController<PortalPreferencesViewModel>
  {
    public PortalPreferencesViewController() : base("PortalPreferencesViewController", null)
    {
    }

    public override void ViewDidLoad()
    {
      base.ViewDidLoad();
      // Perform any additional setup after loading the view, typically from a nib.
      
      var set = CreateBindingSet(); 
      set.Apply();
    }

    public override void DidReceiveMemoryWarning()
    {
      base.DidReceiveMemoryWarning();
      // Release any cached data, images, etc that aren't in use.
    }
  }
}


