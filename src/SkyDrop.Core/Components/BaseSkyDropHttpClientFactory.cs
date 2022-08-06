using System.Collections.Generic;
using System.Net.Http;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.Components
{
    public abstract class BaseSkyDropHttpClientFactory : ISkyDropHttpClientFactory
    {
        /// <summary>
        /// Keep secret!
        /// </summary>
        protected const string MyDevApiKey = "";
        protected const string PortalApiTokenHeader = "Skynet-Api-Key";

        // todo: nb that this might one day cause issues if we stored too many HttpClient instances e.g. for 100s of portals
        protected Dictionary<SkynetPortal, HttpClient> HttpClientsPerPortal { get; private set; } = new Dictionary<SkynetPortal, HttpClient>();

        /// <summary>
        /// This is the default paramaterless implementation for GetSkyDropHttpClientInstance(), it returns the Siasky.net portal.
        /// </summary>
        public HttpClient GetSkyDropHttpClientInstance() => GetSkyDropHttpClientInstance(SkynetPortal.SiaskyPortal);

        /// <summary>
        /// This method returns a HttpClient connected to $portal.BaseUrl.
        /// </summary>
        public abstract HttpClient GetSkyDropHttpClientInstance(SkynetPortal portal);

        public void ClearCachedClients()
        {
            HttpClientsPerPortal = new Dictionary<SkynetPortal, HttpClient>();
        }

        public void UpdateHttpClientWithNewToken(SkynetPortal portal)
        {
            var httpClient = GetSkyDropHttpClientInstance(portal);

            if (!httpClient.DefaultRequestHeaders.Contains(PortalApiTokenHeader))
                AddApiTokenHeader(httpClient, portal);
        }

        protected static void AddApiTokenHeader(HttpClient client, SkynetPortal portal)
        {
            if (portal.HasApiToken())
            {
                client.DefaultRequestHeaders.Add(PortalApiTokenHeader, portal.UserApiToken);
            }
        }
    }
}