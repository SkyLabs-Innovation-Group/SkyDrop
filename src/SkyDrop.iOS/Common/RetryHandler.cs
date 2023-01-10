using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Fody;
using Foundation;
using MvvmCross;

namespace SkyDrop.iOS.Common
{
    public class RetryHandler : NSUrlSessionHandler
    {
        private ILog _log;
        private ILog log => (_log ??= Mvx.IoCProvider.Resolve<ILog>());

        // Strongly consider limiting the number of retries - "retry forever" is
        // probably not the most user friendly way you could respond to "the
        // network cable got pulled out."
        private const int MaxRetries = 5;

        public RetryHandler(NSUrlSessionConfiguration config)
            : base(config)
        { }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            log.Trace("RetryHandler Sending request ");
            log.Trace(request.ToString());
            HttpResponseMessage response = null;
            for (int i = 0; i < MaxRetries; i++)
            {
                log.Trace("Trying upload: try number " + i);

                try
                {
                    response = await base.SendAsync(request, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    log.Error("Error trying request try number " + i);
                    log.Exception(ex);
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

