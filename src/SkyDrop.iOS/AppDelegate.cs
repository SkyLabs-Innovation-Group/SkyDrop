using System;
using System.Threading.Tasks;
using Foundation;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvvmCross;
using MvvmCross.Platforms.Ios.Core;
using SkyDrop.Core;
using UIKit;
using UserNotifications;

namespace SkyDrop.iOS
{
    [Register(nameof(AppDelegate))]
    public class AppDelegate : MvxApplicationDelegate<Setup, App>
    {
        private ILog _log;
        protected ILog log => (_log ??= Mvx.IoCProvider.Resolve<ILog>());

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            var val = base.FinishedLaunching(application, launchOptions);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

            UNUserNotificationCenter.Current.RequestAuthorizationAsync(UNAuthorizationOptions.Announcement);
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;

            AppCenter.Start("382ddca4-6c75-43ee-886c-e533cd272137",
                typeof(Analytics), typeof(Crashes));

            return val;
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogException(e.Exception, nameof(TaskSchedulerOnUnobservedTaskException));
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Error(e.ExceptionObject.ToString());
        }

        private void LogException(System.Exception ex, string errorMessage = "")
        {
            log.Error(errorMessage);
            log.Exception(ex, unhandled: true);
        }
    }
}