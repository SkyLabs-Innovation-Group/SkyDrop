using System.Diagnostics;
using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.IoC;
using MvvmCross.Platforms.Ios.Core;
using MvvmCross.Platforms.Ios.Presenters;
using MvvmCross.ViewModels;
using SkyDrop.Core;

namespace SkyDrop.iOS
{
    public class Setup : MvxIosSetup<App>
    {
        protected override IMvxApplication CreateApp()
        {
            Debug.WriteLine("CreateApp() iOS");

            UserDialogs.Instance = new UserDialogsImpl();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IUserDialogs>(() => UserDialogs.Instance);

            return base.CreateApp();
        }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            base.FillTargetFactories(registry);
        }

        protected override IMvxIosViewPresenter CreateViewPresenter()
        {
            var presenter = base.CreateViewPresenter();
            Mvx.IoCProvider.RegisterSingleton<IMvxIosViewPresenter>(presenter);
            return presenter;
        }
    }
}
