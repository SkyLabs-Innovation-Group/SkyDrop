using System;

using Android.App;
using Android.Runtime;
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
        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Xamarin.Essentials.Platform.Init(this);
        }
    }
}
