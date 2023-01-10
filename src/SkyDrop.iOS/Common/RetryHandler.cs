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

        public RetryHandler(NSUrlSessionConfiguration innerHandler)
            : base(innerHandler)
        { }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            for (int i = 0; i < MaxRetries; i++)
            {
                if (i > 0)
                    log.Error("Retrying upload: try number " + i);
                response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
            }

            return response;
        }
    }
}

