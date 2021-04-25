using System.Diagnostics;
using Acr.UserDialogs;
using SkyDrop.iOS.Bindings;
using MvvmCross;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.IoC;
using MvvmCross.Platforms.Ios.Core;
using MvvmCross.Platforms.Ios.Presenters;
using MvvmCross.ViewModels;
using SkyDrop.Core;
using UIKit;

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
            registry.RegisterCustomBindingFactory<UIView>(ProgressFillHeightBinding.Name, view => new ProgressFillHeightBinding(view));
            registry.RegisterCustomBindingFactory<UIImageView>(ByteArrayImageViewBinding.Name, view => new ByteArrayImageViewBinding(view));

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
