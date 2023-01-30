using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Fody;
using Newtonsoft.Json;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Exceptions;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels.Main;
using Xamarin.Essentials;
using static SkyDrop.Core.Utility.Util;

namespace SkyDrop.Core.Services
{
    [ConfigureAwait(false)]
    public class ApiService : IApiService
    {
        private const string UnauthorizedExceptionMessage = "Unauthorized. Check your API key is set correctly in the Portals screen.";
        private const int SkynetPortalApiTokenLength = 52;
        private readonly IEncryptionService encryptionService;
        private readonly IFileSystemService fileSystemService;
        private readonly ISkyDropHttpClientFactory httpClientFactory;
        private readonly ISingletonService singletonService;
        private readonly IUserDialogs userDialogs;
        private readonly IOpenFolderService openFolderService;

        public CancellationTokenSource UploadCancellationTokenSource { get; private set; } = new CancellationTokenSource();
        public ILog Log { get; }
        public bool DidRequestCancellation { get; private set; }
        public bool IsTestingEndpoint { get; private set; }

        public ApiService(ILog log,
            ISkyDropHttpClientFactory skyDropHttpClientFactory,
            ISingletonService singletonService,
            IUserDialogs userDialogs,
            IFileSystemService fileSystemService,
            IEncryptionService encryptionService,
            IOpenFolderService openFolderService)
        {
            Log = log;
            httpClientFactory = skyDropHttpClientFactory;
            this.singletonService = singletonService;
            this.userDialogs = userDialogs;
            this.fileSystemService = fileSystemService;
            this.encryptionService = encryptionService;
            this.openFolderService = openFolderService;
        }

        public async Task<SkyFile> UploadFile(SkyFile skyfile)
        {
            var fileSizeBytes = skyfile.FileSizeBytes;
            var filename = skyfile.EncryptedFilename ?? skyfile.Filename;

            var portal = SkynetPortal.SelectedPortal.BaseUrl;
            var url = $"{portal}/skynet/skyfile";

            var form = new MultipartFormDataContent();

            using var file = skyfile.GetStream();

            if (fileSizeBytes == 0)
                Log.Error("File size was zero when uploading file");

            form.Add(new StreamContent(file), "file", filename);

            Log.Trace("Sending file " + filename);

            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = form };

            var httpClient = httpClientFactory.GetSkyDropHttpClientInstance(SkynetPortal.SelectedPortal);

            if (!await TestUrl(httpClient, url))
                throw new PortalUnreachableException(portal.ToString(), "Could not reach portal ");

            Log.Trace(request.ToString());

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, UploadCancellationTokenSource.Token);

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
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new Exception(UnauthorizedExceptionMessage);

            if (!response.IsSuccessStatusCode)
                throw new Exception(response.StatusCode.ToString());

            var fileName = GetFilenameFromResponse(response);
            if (fileName == null)
                throw new Exception("Filename cannot be null");

            if (fileName.IsEncryptedFile())
            {
                //save encrypted file
                var encryptedFilePath = await fileSystemService.SaveFile(await response.Content.ReadAsStreamAsync(), fileName, false);

                //decrypt
                var decryptedFilePath = await encryptionService.DecodeFile(encryptedFilePath, true);
                userDialogs.Toast($"Saved {Path.GetFileName(decryptedFilePath)}");
                return;
            }

            //save directly
            using var responseStream = await response.Content.ReadAsStreamAsync();
            var newFilePath = await fileSystemService.SaveToGalleryOrFiles(responseStream, fileName, saveType);

            userDialogs.Toast(new ToastConfig($"Saved {Path.GetFileName(newFilePath)}")
            {
                Action = new ToastAction
                {
                    Text = "Open",
                    TextColor = Colors.Primary,
                    Action = () => openFolderService.OpenFolder(saveType)
                }
            });
        }

        public async Task<(Stream data, string filename)> DownloadFile(string url)
        {
            var httpClient = httpClientFactory.GetSkyDropHttpClientInstance(SkynetPortal.SelectedPortal);
            var response = await httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new Exception(UnauthorizedExceptionMessage);

            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStreamAsync();
            var fileName = GetFilenameFromResponse(response);
            return (data, fileName);
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

            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            var headers = response.Content.Headers;
            var filenameHeader = headers.GetValues("Content-Disposition").FirstOrDefault();

            var filenamePrefix = "filename=\"";
            var startIndex = filenameHeader.IndexOf(filenamePrefix) + filenamePrefix.Length;
            filenameHeader = filenameHeader.Substring(startIndex);
            var filename = filenameHeader.Substring(0, filenameHeader.Length - 1);

            return filename;
        }

        private bool HasValidLength(string apiToken)
        {
            return apiToken.Length == SkynetPortalApiTokenLength;
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

        public CancellationToken GetNewCancellationToken()
        {
            DidRequestCancellation = false; // reset DidRequestCancellation when requesting a new token for new upload attempt
            UploadCancellationTokenSource?.Dispose();
            UploadCancellationTokenSource = null;
            UploadCancellationTokenSource = new CancellationTokenSource();
            return UploadCancellationTokenSource.Token;
        }

        public void CancelUpload()
        {
            DidRequestCancellation = true;
            UploadCancellationTokenSource?.Cancel();
        }

        public async Task<bool> TestUrl(HttpClient httpClient, string url)
        {
            try
            {
                IsTestingEndpoint = true;
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(20000);

                var response = await httpClient.GetAsync(url, cts.Token);
                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.MethodNotAllowed)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (HttpRequestException e)
            {
                return false;
            }
            catch (OperationCanceledException e)
            {
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                IsTestingEndpoint = false;
            }
        }
    }

    public interface IApiService
    {
        Task<SkyFile> UploadFile(SkyFile skyFile);

        Task DownloadAndSaveSkyfile(string url, SaveType saveType);

        Task<(Stream data, string filename)> DownloadFile(string url);

        Task<string> GetSkyFileFilename(string skyfile);

        CancellationToken GetNewCancellationToken();

        void CancelUpload();

        bool DidRequestCancellation { get; }

        bool IsTestingEndpoint { get; }

    }
}