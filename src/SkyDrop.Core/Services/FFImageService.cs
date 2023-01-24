using System.Net.Http;
using System.Threading.Tasks;
using FFImageLoading;
using FFImageLoading.Config;
using FFImageLoading.Helpers;
using FFImageLoading.Work;
using MvvmCross;
using SkyDrop.Core.DataModels;

namespace SkyDrop.Core.Services
{
    public class FfImageService : IFfImageService
    {
        private readonly ISkyDropHttpClientFactory httpClientFactory;
        private readonly ILog log;

        public TaskCompletionSource<bool> InitialiseTask;

        public FfImageService(ILog log, ISkyDropHttpClientFactory skyDropHttpClientFactory)
        {
            this.log = log;
            httpClientFactory = skyDropHttpClientFactory;
        }

        public async Task Initialise()
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
                VerboseLogging = true,
                VerboseLoadingCancelledLogging = true,
                VerbosePerformanceLogging = true,
                VerboseMemoryCacheLogging = true,
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

    public interface IFfImageService
    {
        Task Initialise();
        void UpdateHttpClient(HttpClient httpClient);
    }
}