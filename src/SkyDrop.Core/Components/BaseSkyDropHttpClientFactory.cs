using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using MvvmCross;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Services;
using SkyDrop.Core.Utility;

namespace SkyDrop.Core.Components
{
    public abstract class BaseSkyDropHttpClientFactory : ISkyDropHttpClientFactory
    {
        private ILog _log;
        protected ILog log => (_log ??= Mvx.IoCProvider.Resolve<ILog>());

        public const string PortalApiTokenHeader = "Skynet-Api-Key";

        // todo: nb that this might one day cause issues if we stored too many HttpClient instances e.g. for 100s of portals
        protected Dictionary<SkynetPortal, HttpClient> HttpClientsPerPortal { get; private set; } =
            new Dictionary<SkynetPortal, HttpClient>();

        /// <summary>
        /// This is the default paramaterless implementation for GetSkyDropHttpClientInstance(), it returns the default portal.
        /// </summary>
        public HttpClient GetSkyDropHttpClientInstance()
        {
            return GetSkyDropHttpClientInstance(new SkynetPortal(SkynetPortal.DefaultWeb3PortalUrl));
        }

        /// <summary>
        /// This method returns a HttpClient connected to $portal.BaseUrl.
        /// </summary>
        public abstract HttpClient GetSkyDropHttpClientInstance(SkynetPortal portal);

        public void ClearCachedClients()
        {
            HttpClientsPerPortal = new Dictionary<SkynetPortal, HttpClient>();
        }

        public void UpdateHttpClientWithNewToken(SkynetPortal portal, string apiToken)
        {
            var httpClient = GetSkyDropHttpClientInstance(portal);

            AddApiTokenHeader(httpClient, apiToken);
        }

        public string GetTokenForHttpClient(SkynetPortal portal)
        {
            return GetTokenForHttpClient(GetSkyDropHttpClientInstance(portal));
        }

        public string GetTokenForHttpClient(HttpClient client)
        {
            string apiTokenHeader = null;
            try
            {
                apiTokenHeader = client.DefaultRequestHeaders.Where(c => c.Key == PortalApiTokenHeader).FirstOrDefault()
                    .Value.FirstOrDefault();
            }
            catch (Exception ex)
            {
                log.Exception(ex);   
            }

            return apiTokenHeader;
        }

        protected static void AddApiTokenHeader(HttpClient client, string apiToken)
        {
            client.DefaultRequestHeaders.Remove(PortalApiTokenHeader);
            if (!apiToken.IsNullOrEmpty())
                client.DefaultRequestHeaders.Add(PortalApiTokenHeader, apiToken);
        }

        public void CancelAllUploadsForClient(SkynetPortal portal)
        {
            var log = Mvx.IoCProvider.Resolve<ILog>();
            log.Trace("Cancelling uploads for portal " + portal);
            GetSkyDropHttpClientInstance(portal).CancelPendingRequests();
        }
    }
}