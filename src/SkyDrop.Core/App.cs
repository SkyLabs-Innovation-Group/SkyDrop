using Acr.UserDialogs;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.ViewModels;
using SkyDrop.Core.Services;
using SkyDrop.Core.ViewModels.Main;

namespace SkyDrop.Core
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ILog>(() => new SkyLogger());

            AppCenter.Start("a4cdd96b-271f-4337-a1a6-57801ea8fd9c",
                   typeof(Analytics), typeof(Crashes));

            RegisterAppStart<MenuViewModel>();
        }
    }
}
