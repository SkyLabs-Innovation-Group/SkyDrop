using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Fody;
using Newtonsoft.Json;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;

namespace SkyDrop.Core.Services
{
    [ConfigureAwait(false)]
    public class ApiService : IApiService
    {
        public ILog Log { get; }
        
        private HttpClient httpClient;

        public ApiService(ILog log, ISkyDropHttpClientFactory skyDropHttpClientFactory)
        {
            Log = log;
            httpClient = skyDropHttpClientFactory.GetSkyDropHttpClientInstance();
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
    }

    public interface IApiService
    {
        Task<SkyFile> UploadFile(SkyFile skyFile, CancellationTokenSource cancellationTokenSource);
    }
}
