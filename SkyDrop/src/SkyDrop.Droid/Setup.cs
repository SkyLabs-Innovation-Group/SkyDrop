using System;
using System.Diagnostics;
using Acr.UserDialogs;
using AndroidX.CardView.Widget;
using Engage.Droid.Bindings;
using MvvmCross;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.IoC;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Core;
using MvvmCross.ViewModels;
using SkyDrop.Core;

namespace SkyDrop.Droid
{
    public class Setup : MvxAndroidSetup<App>
    {
        private IMvxAndroidCurrentTopActivity topActivityProvider { get; set; }

        protected override IMvxApplication CreateApp()
        {
            Debug.WriteLine("CreateApp() droid");

            while (topActivityProvider.Activity == null)
            {

            }

            UserDialogs.Init(topActivityProvider.Activity);

            Mvx.IoCProvider.LazyConstructAndRegisterSingleton(() => UserDialogs.Instance);

            return base.CreateApp();
        }

        protected override IMvxAndroidCurrentTopActivity CreateAndroidCurrentTopActivity()
        {
            Debug.WriteLine("CreateAndroidCurrentTopActivity() droid");

            topActivityProvider = base.CreateAndroidCurrentTopActivity();

            return topActivityProvider;
        }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            base.FillTargetFactories(registry);
            registry.RegisterCustomBindingFactory<CardView>(CardBackgroundColorBinding.Name, view => new CardBackgroundColorBinding(view));
        }
    }
}
