using System.Net.Http;

namespace SkyDrop.Core.Services
{

    
    public interface ISkyDropHttpClientFactory
    {
        HttpClient GetSkyDropHttpClientInstance();
    }
}