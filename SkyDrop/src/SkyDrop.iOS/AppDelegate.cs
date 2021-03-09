using Foundation;
using MvvmCross.Platforms.Ios.Core;
using SkyDrop.Core;

namespace SkyDrop.iOS
{
    [Register(nameof(AppDelegate))]
    public class AppDelegate : MvxApplicationDelegate<Setup, App>
    {
    }
}
