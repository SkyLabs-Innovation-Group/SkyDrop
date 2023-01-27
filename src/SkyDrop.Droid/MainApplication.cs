using System;
using Android.App;
using Android.Runtime;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvvmCross;
using MvvmCross.Platforms.Android.Views;
using SkyDrop.Core;
using Xamarin.Essentials;

namespace SkyDrop.Droid
{
#if DEBUG
    [Application(Debuggable = true, Icon = "@mipmap/ic_launcher", RoundIcon = "@mipmap/ic_launcher_round")]
#else
    [Application(Debuggable = false, Icon = "@mipmap/ic_launcher", RoundIcon = "@mipmap/ic_launcher_round")]
#endif

    public class MainApplication : MvxAndroidApplication<Setup, App>
    {
        public const string AndroidAppCenterApiKey = "a4cdd96b-271f-4337-a1a6-57801ea8fd9c";

        private ILog _log;
        protected ILog log => (_log ??= Mvx.IoCProvider.Resolve<ILog>());

        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironmentOnUnhandledExceptionRaiser;

            Platform.Init(this);

            AppCenter.Start(AndroidAppCenterApiKey,
                typeof(Analytics), typeof(Crashes));
        }

        private void AndroidEnvironmentOnUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            log.Error("Unhandled exception: ");
            log.Exception(e.Exception, unhandled: true);
        }
    }
}