using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Fody;
using Newtonsoft.Json;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;
using Xamarin.Essentials;

namespace SkyDrop.Core.Services
{
    [ConfigureAwait(false)]
    public class ApiService : IApiService
    {
        public ILog Log { get; }
        
        private ISkyDropHttpClientFactory httpClientFactory;
        private ISingletonService singletonService;
        private IUserDialogs userDialogs;

        public ApiService(ILog log,
            ISkyDropHttpClientFactory skyDropHttpClientFactory,
            ISingletonService singletonService,
            IUserDialogs userDialogs)
        {
            Log = log;
            httpClientFactory = skyDropHttpClientFactory;
            this.singletonService = singletonService;
            this.userDialogs = userDialogs;
        }
        
        public async Task<SkyFile> UploadFile(SkyFile skyfile, CancellationTokenSource cancellationTokenSource)
        {
            var fileSizeBytes = skyfile.FileSizeBytes;
            var filename = skyfile.Filename;

            var url = $"{SkynetPortal.SelectedPortal}/skynet/skyfile";

            var form = new MultipartFormDataContent();
            
            using var file = skyfile.GetStream();
            
            if (fileSizeBytes == 0)
                Log.Error("File size was zero when uploading file");
            
            form.Add(new StreamContent(file), "file", filename);
            
            Log.Trace("Sending file " + filename);
            
            var request = new HttpRequestMessage(HttpMethod.Post, url) {Content =  form};
            
            Log.Trace(request.ToString());

            var httpClient = httpClientFactory.GetSkyDropHttpClientInstance(SkynetPortal.SelectedPortal);
            
            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationTokenSource.Token);
            
            response.EnsureSuccessStatusCode();
            
            var responseString = await response.Content.ReadAsStringAsync();
            Log.Trace(responseString);

            var skyFile = JsonConvert.DeserializeObject<SkyFile>(responseString);

            if (skyfile == null)
                throw new ArgumentNullException(nameof(skyfile));

            // From ReSharper - in debug mode, outputs call stack + message here when this condition is false
            Debug.Assert(skyFile != null, nameof(skyFile) + " != null");
            
            skyFile.Filename = filename;
            skyFile.Status = FileStatus.Uploaded;
            skyFile.FileSizeBytes = fileSizeBytes;

            return skyFile;
        }

        public async Task<string> GetSkyFileFilename(SkyFile skyfile)
        {
            var httpClient = httpClientFactory.GetSkyDropHttpClientInstance(SkynetPortal.SelectedPortal);
            var request = new HttpRequestMessage(HttpMethod.Head, skyfile.GetSkylinkUrl());

            var result = await httpClient.SendAsync(request);
            if (result == null)
                return null;

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            var headers = result.Content.Headers;
            var filenameHeader = headers.GetValues("Content-Disposition").FirstOrDefault();

            var filenamePrefix = "filename=\"";
            var startIndex = filenameHeader.IndexOf(filenamePrefix) + filenamePrefix.Length;
            filenameHeader = filenameHeader.Substring(startIndex);
            var filename = filenameHeader.Substring(0, filenameHeader.Length - 1);

            return filename;
        }

        public async Task<bool> PingPortalForSkylink(string skylink, SkynetPortal skynetPortal)
        {
            var httpClient = httpClientFactory.GetSkyDropHttpClientInstance(skynetPortal);

            string requestUrl = $"{skynetPortal}/{skylink}";

            var request = new HttpRequestMessage(HttpMethod.Head, requestUrl);

            HttpResponseMessage result = null;
            try
            {
                result = await httpClient.SendAsync(request);
            }
            catch (HttpRequestException httpEx) when (httpEx.Message.Contains("SSL") && DeviceInfo.Platform == DevicePlatform.Android)
            {
                userDialogs.Alert(Strings.SslPrompt);
                Log.Exception(httpEx);
                return false;
            }
            catch (HttpRequestException e)
            {
                Log.Error("Error pinging skynet portal at " + skynetPortal.BaseUrl);
                Log.Exception(e);
            }

            if (result == null)
            {
                userDialogs.Toast("No response from " + skynetPortal);
                Log.Error($"Head request to {skynetPortal} returned null");
                return false;
            }

            Log.Trace(result.ToString());

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                userDialogs.Toast($"{skynetPortal} refused the portal check request");
                Log.Error($"Head request to {skynetPortal} returned status code {result.StatusCode}");
                return false;
            }

            var headers = result.Headers;
            var skylinkHeader = headers.GetValues("Skynet-Skylink").FirstOrDefault();
            if (!(skylinkHeader == skylink))
            {
                Log.Error("!(skylinkHeader == skylink)");
                userDialogs.Toast($"{skynetPortal} Skylink Header did not match");
                return false;
            }

            Log.Trace("Success querying for file header on portal " + skynetPortal);
            return true;
        }
    }

    public interface IApiService
    {
        Task<SkyFile> UploadFile(SkyFile skyFile, CancellationTokenSource cancellationTokenSource);

        Task<string> GetSkyFileFilename(SkyFile skyfile);

        Task<bool> PingPortalForSkylink(string skylink, SkynetPortal skynetPortal);
    }
}
