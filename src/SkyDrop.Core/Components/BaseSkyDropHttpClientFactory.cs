using System.Collections.Generic;
using System.Net.Http;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.Components
{
    public abstract class BaseSkyDropHttpClientFactory : ISkyDropHttpClientFactory
    {
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
    }
}