using System;
using System.Diagnostics;
using System.IO;
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
using static SkyDrop.Core.Utility.Util;

namespace SkyDrop.Core.Services
{
    [ConfigureAwait(false)]
    public class ApiService : IApiService
    {
        public ILog Log { get; }

        private readonly IFileSystemService fileSystemService;
        private readonly ISkyDropHttpClientFactory httpClientFactory;
        private readonly ISingletonService singletonService;
        private readonly IUserDialogs userDialogs;
        private readonly IEncryptionService encryptionService;

        public ApiService(ILog log,
            ISkyDropHttpClientFactory skyDropHttpClientFactory,
            ISingletonService singletonService,
            IUserDialogs userDialogs,
            IFileSystemService fileSystemService,
            IEncryptionService encryptionService)
        {
            Log = log;
            httpClientFactory = skyDropHttpClientFactory;
            this.singletonService = singletonService;
            this.userDialogs = userDialogs;
            this.fileSystemService = fileSystemService;
            this.encryptionService = encryptionService;
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
            
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = form };
            
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

        public async Task DownloadAndSaveSkyfile(string url, SaveType saveType)
        {
            var permissionResult = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (permissionResult != PermissionStatus.Granted)
            {
                userDialogs.Toast("Storage permission not granted.");
                return;
            }

            //download
            var httpClient = httpClientFactory.GetSkyDropHttpClientInstance(SkynetPortal.SelectedPortal);
            var response = await httpClient.GetAsync(url);
            
            var fileName = GetFilenameFromResponse(response);
            if (fileName.ExtensionMatches(".skydrop"))
            {
                //save encrypted file
                var encryptedFilePath = await fileSystemService.SaveFile(await response.Content.ReadAsStreamAsync(), fileName, false);

                //decrypt
                var decryptedFilePath = await encryptionService.DecodeFile(encryptedFilePath);
                userDialogs.Toast($"Saved {Path.GetFileName(decryptedFilePath)}");
                return;
            }

            //save directly
            using var responseStream = await response.Content.ReadAsStreamAsync();
            var newFilePath = await fileSystemService.SaveToGalleryOrFiles(responseStream, fileName, saveType);

            userDialogs.Toast($"Saved {Path.GetFileName(newFilePath)}");
        }

        public async Task<Stream> DownloadFile(string url)
        {
            //download
            var httpClient = httpClientFactory.GetSkyDropHttpClientInstance(SkynetPortal.SelectedPortal);
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }

        public async Task<string> GetSkyFileFilename(string skylink)
        {
            var httpClient = httpClientFactory.GetSkyDropHttpClientInstance(SkynetPortal.SelectedPortal);
            var request = new HttpRequestMessage(HttpMethod.Head, skylink);
            var response = await httpClient.SendAsync(request);
            return GetFilenameFromResponse(response);
        }

        private string GetFilenameFromResponse(HttpResponseMessage response)
        {
            if (response == null)
                return null;

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            var headers = response.Content.Headers;
            var filenameHeader = headers.GetValues("Content-Disposition").FirstOrDefault();

            var filenamePrefix = "filename=\"";
            var startIndex = filenameHeader.IndexOf(filenamePrefix) + filenamePrefix.Length;
            filenameHeader = filenameHeader.Substring(startIndex);
            var filename = filenameHeader.Substring(0, filenameHeader.Length - 1);

            return filename;
        }

        private const int SkynetPortalApiTokenLength = 52;

        private bool HasValidLength(string apiToken) => apiToken.Length == SkynetPortalApiTokenLength;

        public async Task<bool> PingPortalForSkylink(string skylink, SkynetPortal skynetPortal)
        {
            string oldToken = null;
            if (skynetPortal.HasApiToken() && !HasValidLength(skynetPortal.UserApiToken))
            {
                userDialogs.Toast("The API token entered was invalid");
                Log.Error($"The API token entered was invalid");
                return FailedPortalCheck(skynetPortal, oldToken);
            }
            else if (skynetPortal.HasApiToken())
            {
                // Store oldToken while validating the new token using a HEAD request, swap back in FailedPortalCheck()
                oldToken = httpClientFactory.GetTokenForHttpClient(skynetPortal);

                if (oldToken == skynetPortal.UserApiToken) // if oldToken == newToken, treat as a new token
                    oldToken = null;

                httpClientFactory.UpdateHttpClientWithNewToken(skynetPortal);
            }

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
                return FailedPortalCheck(skynetPortal, oldToken);
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
                return FailedPortalCheck(skynetPortal, oldToken);
            }

            Log.Trace(result.ToString());

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                userDialogs.Toast($"{skynetPortal} refused the portal check request");
                Log.Error($"Head request to {skynetPortal} returned status code {result.StatusCode}");
                return FailedPortalCheck(skynetPortal, oldToken);
            }

            var headers = result.Headers;
            var skylinkHeader = headers.GetValues("Skynet-Skylink").FirstOrDefault();
            if (!(skylinkHeader == skylink))
            {
                Log.Error("!(skylinkHeader == skylink)");
                userDialogs.Toast($"{skynetPortal} Skylink Header did not match");
                return FailedPortalCheck(skynetPortal, oldToken);
            }

            Log.Trace("Success querying for file header on portal " + skynetPortal);
            return true;
        }

        public bool FailedPortalCheck(SkynetPortal portal, string oldToken)
        {
            if (string.IsNullOrEmpty(oldToken))
                return false;

            // Set back to valid old token if new one failed
            portal.UserApiToken = oldToken;
            httpClientFactory.UpdateHttpClientWithNewToken(portal);

            return false;
        }
    }

    public interface IApiService
    {
        Task<SkyFile> UploadFile(SkyFile skyFile, CancellationTokenSource cancellationTokenSource);

        Task DownloadAndSaveSkyfile(string url, SaveType saveType);

        Task<Stream> DownloadFile(string url);

        Task<string> GetSkyFileFilename(string skyfile);

        Task<bool> PingPortalForSkylink(string skylink, SkynetPortal skynetPortal);
    }
}
