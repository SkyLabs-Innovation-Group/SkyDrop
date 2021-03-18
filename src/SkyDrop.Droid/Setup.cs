using System;
using System.Diagnostics;
using Acr.UserDialogs;
using Android.Views;
using AndroidX.CardView.Widget;
using Engage.Droid.Bindings;
using Google.Android.Material.Card;
using MvvmCross;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.IoC;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Core;
using MvvmCross.ViewModels;
using SkyDrop.Core;
using SkyDrop.Droid.Bindings;

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
            registry.RegisterCustomBindingFactory<MaterialCardView>(MaterialCardStateBinding.Name, view => new MaterialCardStateBinding(view));
            registry.RegisterCustomBindingFactory<MaterialCardView>(BarcodeBackgroundBinding.Name, view => new BarcodeBackgroundBinding(view));
            registry.RegisterCustomBindingFactory<View>(ViewHiddenBinding.Name, view => new ViewHiddenBinding(view));
        }
    }
}
