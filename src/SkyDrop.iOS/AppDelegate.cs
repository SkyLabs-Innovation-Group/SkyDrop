using Foundation;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvvmCross.Platforms.Ios.Core;
using SkyDrop.Core;
using UIKit;
using UserNotifications;

namespace SkyDrop.iOS
{
    [Register(nameof(AppDelegate))]
    public class AppDelegate : MvxApplicationDelegate<Setup, App>
    {
        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            var val = base.FinishedLaunching(application, launchOptions);

            UNUserNotificationCenter.Current.RequestAuthorizationAsync(UNAuthorizationOptions.Announcement);
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;

            AppCenter.Start("382ddca4-6c75-43ee-886c-e533cd272137",
                typeof(Analytics), typeof(Crashes));

            return val;
        }
    }
}