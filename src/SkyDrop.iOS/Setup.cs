using System.Diagnostics;
using System.IO;
using Acr.UserDialogs;
using FFImageLoading;
using FFImageLoading.Config;
using FFImageLoading.Helpers;
using FFImageLoading.Work;
using SkyDrop.iOS.Bindings;
using MvvmCross;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.IoC;
using MvvmCross.Platforms.Ios.Core;
using MvvmCross.Platforms.Ios.Presenters;
using MvvmCross.ViewModels;
using SkyDrop.Core;
using UIKit;
using MvvmCross.Converters;
using MvvmCross.Logging;
using Serilog;
using SkyDrop.Core.Converters;
using SkyDrop.Core.Services;
using SkyDrop.iOS.Services;
using Xamarin.Essentials;
using SkyDrop.iOS.Converters;

namespace SkyDrop.iOS
{
    public class Setup : MvxIosSetup<App>
    {
        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterCustomBindingFactory<UIView>(ProgressFillHeightBinding.Name, view => new ProgressFillHeightBinding(view));
            registry.RegisterCustomBindingFactory<UIImageView>(SkyFileImageViewBinding.Name, view => new SkyFileImageViewBinding(view));

            base.FillTargetFactories(registry);
        }

        protected override void FillValueConverters(IMvxValueConverterRegistry registry)
        {
            registry.AddOrOverwrite(FileExtensionConverter.Name, new FileExtensionConverter());
            registry.AddOrOverwrite(NativeColorConverter.Name, new NativeColorConverter());
            registry.AddOrOverwrite(CGColorConverter.Name, new CGColorConverter());

            base.FillValueConverters(registry);
        }

        protected override IMvxApplication CreateApp()
        {
            Debug.WriteLine("CreateApp() iOS");

            UserDialogs.Instance = new UserDialogsImpl();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IUserDialogs>(() => UserDialogs.Instance);

            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ISkyDropHttpClientFactory>(() => new NSUrlHttpClientFactory());
            
            return base.CreateApp();
        }
        
        protected override IMvxIosViewPresenter CreateViewPresenter()
        {
            var presenter = base.CreateViewPresenter();
            Mvx.IoCProvider.RegisterSingleton<IMvxIosViewPresenter>(presenter);
            return presenter;
        }
        
        public override MvxLogProviderType GetDefaultLogProviderType() => MvxLogProviderType.Serilog;

        protected override IMvxLogProvider CreateLogProvider()
        {
            // From https://prin53.medium.com/logging-in-xamarin-application-logging-infrastructure-with-mvvmcross-2c9bef960c60
            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.NSLog(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} ({SourceContext}) {Exception}")
                .WriteTo.File(
                    Path.Combine(FileSystem.CacheDirectory, "Logs", "Log.log"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} ({SourceContext}) {Exception}{NewLine}"
                ).CreateLogger();
            
            var logProvider = base.CreateLogProvider();
            
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ILog>(() => new SkyLogger(logProvider));

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
