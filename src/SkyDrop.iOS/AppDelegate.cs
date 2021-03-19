using Acr.UserDialogs;
using Foundation;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.Platforms.Ios.Core;
using SkyDrop.Core;
using UIKit;

namespace SkyDrop.iOS
{
    [Register(nameof(AppDelegate))]
    public class AppDelegate : MvxApplicationDelegate<Setup, App>
    {
        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            AppCenter.Start("382ddca4-6c75-43ee-886c-e533cd272137",
                               typeof(Analytics), typeof(Crashes));


            return base.FinishedLaunching(application, launchOptions);
        }
    }
}
