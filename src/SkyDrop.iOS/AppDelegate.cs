using Acr.UserDialogs;
using Foundation;
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

    }
}
