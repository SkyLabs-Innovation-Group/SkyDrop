using System;
using System.Net.Http;
using System.Threading.Tasks;
using FFImageLoading;
using FFImageLoading.Config;
using FFImageLoading.Helpers;
using FFImageLoading.Work;
using MvvmCross;
using SkyDrop.Core.DataModels;
using Xamarin.Essentials;

namespace SkyDrop.Core.Services
{
    public class FFImageService : IFFImageService
    {
        public FFImageService(ILog log, ISkyDropHttpClientFactory skyDropHttpClientFactory)
        {
            this.log = log;
            this.httpClientFactory = skyDropHttpClientFactory;
        }

        public TaskCompletionSource<bool> InitialiseTask;
        private readonly ILog log;
        private readonly ISkyDropHttpClientFactory httpClientFactory;

        public async Task Initialise()
        {
            if (InitialiseTask == null)
                InitialiseTask = new TaskCompletionSource<bool>();
            else 
                return;

            var selectedPortal = SkynetPortal.SelectedPortal;

            ImageService.Instance.Initialize(new Configuration()
            {
                ClearMemoryCacheOnOutOfMemory = true,
                DownsampleInterpolationMode = InterpolationMode.Low,

                // Logging attributes 
                Logger = (IMiniLogger)Mvx.IoCProvider.Resolve<ILog>(),
                VerboseLogging = true,
                VerboseLoadingCancelledLogging = true,
                VerbosePerformanceLogging = true,
                VerboseMemoryCacheLogging = true,
                HttpClient = httpClientFactory.GetSkyDropHttpClientInstance(selectedPortal),
            });

            ImageService.Instance.Config.Logger = (IMiniLogger)Mvx.IoCProvider.Resolve<ILog>();

            InitialiseTask.SetResult(true);
        }

        public void UpdateHttpClient(HttpClient httpClient)
        {
            ImageService.Instance.Config.HttpClient = httpClient;
        }
    }

    public interface IFFImageService
    {
        Task Initialise();
        void UpdateHttpClient(HttpClient httpClient);
    }
}

