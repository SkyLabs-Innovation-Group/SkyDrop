using System;

using Android.App;
using Android.Runtime;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvvmCross.Platforms.Android.Views;
using SkyDrop.Core;

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

        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Xamarin.Essentials.Platform.Init(this);

            AppCenter.Start(AndroidAppCenterApiKey,
                   typeof(Analytics), typeof(Crashes));
        }
    }
}
