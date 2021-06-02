using System;
using System.Diagnostics;
using System.IO;
using Acr.UserDialogs;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using FFImageLoading;
using FFImageLoading.Config;
using FFImageLoading.Cross;
using FFImageLoading.Helpers;
using FFImageLoading.Work;
using SkyDrop.Droid.Bindings;
using Google.Android.Material.Card;
using MvvmCross;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.IoC;
using MvvmCross.Logging;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Core;
using MvvmCross.ViewModels;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using SkyDrop.Core;
using SkyDrop.Core.Services;
using SkyDrop.Droid.Bindings;
using SkyDrop.Droid.Services;
using Xamarin.Essentials;
using Log = Acr.UserDialogs.Infrastructure.Log;

namespace SkyDrop.Droid
{
    public class Setup : MvxAndroidSetup<App>
    {
        private IMvxAndroidCurrentTopActivity topActivityProvider { get; set; }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            base.FillTargetFactories(registry);
            registry.RegisterCustomBindingFactory<CardView>(CardBackgroundColorBinding.Name, view => new CardBackgroundColorBinding(view));
            registry.RegisterCustomBindingFactory<MaterialCardView>(MaterialCardStateBinding.Name, view => new MaterialCardStateBinding(view));
            registry.RegisterCustomBindingFactory<MaterialCardView>(BarcodeBackgroundBinding.Name, view => new BarcodeBackgroundBinding(view));
            registry.RegisterCustomBindingFactory<View>(VisibleHiddenBinding.Name, view => new VisibleHiddenBinding(view));
            registry.RegisterCustomBindingFactory<ProgressBar>(UploadProgressBinding.Name, view => new UploadProgressBinding(view));
            registry.RegisterCustomBindingFactory<MvxCachedImageView>(ImagePreviewBinding.Name, view => new ImagePreviewBinding(view));
        }
        
        protected override IMvxApplication CreateApp()
        {
            Debug.WriteLine("CreateApp() droid");
            
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
        
        public override MvxLogProviderType GetDefaultLogProviderType() => MvxLogProviderType.Serilog;

        protected override IMvxLogProvider CreateLogProvider()
        {
            // From https://prin53.medium.com/logging-in-xamarin-application-logging-infrastructure-with-mvvmcross-2c9bef960c60
            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.AndroidLog(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} ({SourceContext}) {Exception}")
                .WriteTo.File(
                    Path.Combine(FileSystem.CacheDirectory, "Logs", "Log.log"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} ({SourceContext}) {Exception}{NewLine}"
                ).CreateLogger();
            
            var logProvider = base.CreateLogProvider();
            
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ILog>(() => new SkyLogger(logProvider));
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ISkyDropHttpClientFactory>(() => new AndroidHttpClientFactory());

            ImageService.Instance.Initialize(new Configuration()
            {
                ClearMemoryCacheOnOutOfMemory = true,
                DownsampleInterpolationMode = InterpolationMode.Low,
                
                // Logging attributes 
                Logger = (IMiniLogger) Mvx.IoCProvider.Resolve<ILog>(),
                // VerboseLogging = true,
                // VerboseLoadingCancelledLogging = true,
                // VerbosePerformanceLogging = true,
                // VerboseMemoryCacheLogging = true,
            });

            ImageService.Instance.Config.Logger = (IMiniLogger) Mvx.IoCProvider.Resolve<ILog>();

            return logProvider;
        }

  
    }
}
