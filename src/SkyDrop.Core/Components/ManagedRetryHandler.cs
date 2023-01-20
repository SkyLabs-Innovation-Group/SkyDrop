using System;
using MvvmCross;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.Components
{
    public class ManagedRetryHandler : HttpClientHandler
    {
        private ILog _log;
        private ILog log => (_log ??= Mvx.IoCProvider.Resolve<ILog>());

        private IApiService _apiService;
        private IApiService apiService => (_apiService ??= Mvx.IoCProvider.Resolve<IApiService>());

        // Strongly consider limiting the number of retries - "retry forever" is
        // probably not the most user friendly way you could respond to "the
        // network cable got pulled out."
        private const int MaxRetries = 5;

        public ManagedRetryHandler() : base()
        { }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            log.Trace("RetryHandler Sending request ");
            log.Trace(request.ToString());
            HttpResponseMessage response = null;
            for (int i = 1; i < MaxRetries; i++)
            {
                log.Trace("Trying upload: try number " + i);

                try
                { 
                    // Switch these to test with the cancellationToken enabled
                    response = await base.SendAsync(request, cancellationToken);
                }
                catch (TaskCanceledException tce)
                {
                    if (apiService.DidRequestCancellation)
                        throw;

                    log.Error("Error trying request try number " + i);
                    log.Exception(tce);
                    cancellationToken = apiService.GetNewCancellationToken();
                }
                catch (Exception ex)
                {
                    log.Error("Error trying request try number " + i);
                    log.Exception(ex);
                    cancellationToken = apiService.GetNewCancellationToken();
                }

                if (response?.IsSuccessStatusCode ?? false)
                {
                    return response;
                }
            }

            return response;
        }
    }
}

