using System.Diagnostics;
using System.IO;
using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.Converters;
using MvvmCross.IoC;
using MvvmCross.Logging;
using MvvmCross.Platforms.Ios.Core;
using MvvmCross.Platforms.Ios.Presenters;
using MvvmCross.ViewModels;
using Serilog;
using SkyDrop.Core;
using SkyDrop.Core.Converters;
using SkyDrop.Core.Services;
using SkyDrop.iOS.Bindings;
using SkyDrop.iOS.Converters;
using SkyDrop.iOS.Services;
using UIKit;
using Xamarin.Essentials;

namespace SkyDrop.iOS
{
    public class Setup : MvxIosSetup<App>
    {
        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterCustomBindingFactory<UIView>(ProgressFillHeightBinding.Name,
                view => new ProgressFillHeightBinding(view));
            registry.RegisterCustomBindingFactory<UIImageView>(LocalImagePreviewBinding.Name,
                view => new LocalImagePreviewBinding(view));
            registry.RegisterCustomBindingFactory<UIView>(LongPressBinding.Name, view => new LongPressBinding(view));
            registry.RegisterCustomBindingFactory<UIImageView>(FileCategoryIconBinding.Name,
                view => new FileCategoryIconBinding(view));
            registry.RegisterCustomBindingFactory<UIImageView>(IconBinding.Name, view => new IconBinding(view));
            registry.RegisterCustomBindingFactory<UIView>(NextButtonStyleBinding.Name,
                view => new NextButtonStyleBinding(view));

            base.FillTargetFactories(registry);
        }

        protected override void FillValueConverters(IMvxValueConverterRegistry registry)
        {
            registry.AddOrOverwrite(FileExtensionConverter.Name, new FileExtensionConverter());
            registry.AddOrOverwrite(NativeColorConverter.Name, new NativeColorConverter());
            registry.AddOrOverwrite(CanDisplayPreviewConverter.Name, new CanDisplayPreviewConverter(false));
            registry.AddOrOverwrite(CanDisplayPreviewConverter.InvertName, new CanDisplayPreviewConverter(true));
            registry.AddOrOverwrite(SaveUnzipIconConverter.Name, new SaveUnzipIconConverter());

            base.FillValueConverters(registry);
        }

        protected override IMvxApplication CreateApp()
        {
            Debug.WriteLine("CreateApp() iOS");

            UserDialogs.Instance = new UserDialogsImpl();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton(() => UserDialogs.Instance);
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ISkyDropHttpClientFactory>(() =>
                new NSUrlHttpClientFactory());
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ISaveToGalleryService>(
                () => new iOSSaveToGalleryService());

            return base.CreateApp();
        }

        protected override IMvxIosViewPresenter CreateViewPresenter()
        {
            var presenter = base.CreateViewPresenter();
            Mvx.IoCProvider.RegisterSingleton(presenter);
            return presenter;
        }

        public override MvxLogProviderType GetDefaultLogProviderType()
        {
            return MvxLogProviderType.Serilog;
        }

        protected override IMvxLogProvider CreateLogProvider()
        {
            // From https://prin53.medium.com/logging-in-xamarin-application-logging-infrastructure-with-mvvmcross-2c9bef960c60
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.NSLog(
                    outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} ({SourceContext}) {Exception}")
                .WriteTo.File(
                    Path.Combine(FileSystem.CacheDirectory, "Logs", "Log.log"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate:
                    "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} ({SourceContext}) {Exception}{NewLine}"
                ).CreateLogger();

            var logProvider = base.CreateLogProvider();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ILog>(() => new SkyLogger(logProvider));
            return logProvider;
        }
    }
}