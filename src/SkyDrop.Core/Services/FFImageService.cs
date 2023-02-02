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
        private readonly ISkyDropHttpClientFactory httpClientFactory;
        private readonly ILog log;

        public TaskCompletionSource<bool> InitialiseTask;

        public FFImageService(ILog log, ISkyDropHttpClientFactory skyDropHttpClientFactory)
        {
            this.log = log;
            httpClientFactory = skyDropHttpClientFactory;
        }

        public void Initialize()
        {
            if (InitialiseTask == null)
                InitialiseTask = new TaskCompletionSource<bool>();
            else
                return;

            var selectedPortal = SkynetPortal.SelectedPortal;
            
            ImageService.Instance.Initialize(new Configuration
            {
                ClearMemoryCacheOnOutOfMemory = true,
                DownsampleInterpolationMode = InterpolationMode.Low,

                // Logging attributes 
                Logger = (IMiniLogger)Mvx.IoCProvider.Resolve<ILog>(),
                //VerboseLogging = true,
                //VerboseLoadingCancelledLogging = true,
                //VerbosePerformanceLogging = true,
                //VerboseMemoryCacheLogging = true,
                HttpClient = httpClientFactory.GetSkyDropHttpClientInstance(selectedPortal)
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
        void Initialize();
        void UpdateHttpClient(HttpClient httpClient);
    }
}