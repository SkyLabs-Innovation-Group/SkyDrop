using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Http;
using SkyDrop.Core.Utility;

namespace SkyDrop.Core.Services
{
    public class ApiService : IApiService
    {
        public ILog Log { get; }

        public event EventHandler<(long uploaded, long size)> UploadProgressUpdate;

        public ApiService(ILog log)
        {
            Log = log;
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://siasky.net/");
        }

        private HttpClient httpClient { get; set; }

        private ProgressStreamContent GetUploadFileRequest(string filename, byte[] file, long fileSizeBytes)
        {
            var form = new MultipartFormDataContent();
            form.Add(new ByteArrayContent(file), "file", filename);

            //convert form to ProgressStreamContent so we can track upload progress
            var requestContent = new ProgressStreamContent(form);
            requestContent.ProgressUpdate += UploadProgressUpdate;

            return requestContent;
        }

        //TODO: try using HttpClientRequest to disable request buffering
        //https://docs.microsoft.com/en-us/dotnet/api/system.net.httpwebrequest.allowwritestreambuffering?redirectedfrom=MSDN&view=net-5.0#System_Net_HttpWebRequest_AllowWriteStreamBuffering
        public async Task<SkyFile> UploadFile(string filename, byte[] file, long fileSizeBytes)
        {
            if (fileSizeBytes == 0)
                Log.Error("File size was zero when uploading file");

            var url = $"{Util.Portal}/skynet/skyfile";
            var requestContent = GetUploadFileRequest(filename, file, fileSizeBytes);

            Log.Trace("Sending file " + filename);

            var response = await httpClient.PostAsync(url, requestContent).ConfigureAwait(false);

            Log.Trace(response.RequestMessage.ToString());

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var skyFile = JsonConvert.DeserializeObject<SkyFile>(responseString);
            skyFile.Filename = filename;
            skyFile.Status = FileStatus.Uploaded;
            skyFile.FileSizeBytes = fileSizeBytes;

            return skyFile;
        }

        public async Task<SkyFile> UploadFile2(string filename, byte[] file, long fileSizeBytes)
        {
            var url = $"{Util.Portal}/skynet/skyfile";

            // Create a new 'HttpWebRequest' object to the mentioned Uri.				
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            // Set AllowWriteStreamBuffering to 'false'.
            myHttpWebRequest.AllowWriteStreamBuffering = false;

            //Console.WriteLine("\nPlease Enter the data to be posted to the (http://www.contoso.com/codesnippets/next.asp) uri:");
            //string inputData = Console.ReadLine();
            //string postData = "firstone=" + inputData;

            // Set 'Method' property of 'HttpWebRequest' class to POST.
            myHttpWebRequest.Method = "POST";
            //ASCIIEncoding encodedData = new ASCIIEncoding();
            //byte[] byteArray = encodedData.GetBytes(postData);

            var requestContent = GetUploadFileRequest(filename, file, fileSizeBytes);
            //byte[] byteArray = await requestContent.ReadAsByteArrayAsync();

            // Set 'ContentType' property of the 'HttpWebRequest' class to "application/x-www-form-urlencoded".
            myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";

            // If the AllowWriteStreamBuffering property of HttpWebRequest is set to false,the contentlength has to be set to length of data to be posted else Exception(411) is raised.
            var contentLength = requestContent.Headers.ContentLength;
            myHttpWebRequest.ContentLength = contentLength ?? throw new Exception("No content length set");//requestContent.FileSizeBytes;//byteArray.Length;

            var uploadStream = myHttpWebRequest.GetRequestStream();


            //await requestContent.CopyToAsync(newStream);

            await requestContent.PushToStreamAsync(uploadStream);

            //newStream.Write(byteArray, 0, byteArray.Length);
            uploadStream.Close();
            Console.WriteLine("\nData has been posted to the Uri\n\nPlease wait for the response..........");

            // Assign the response object of 'HttpWebRequest' to a 'HttpWebResponse' variable.
            HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();

            var responseStream = myHttpWebResponse.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            string responseString = reader.ReadToEnd();
            //var skyFile = JsonConvert.DeserializeObject<SkyFile>(responseString);
            //skyFile.Filename = filename;
            //skyFile.Status = FileStatus.Uploaded;
            //skyFile.FileSizeBytes = fileSizeBytes;

            Log.Trace("************** YES ITSS ME: " + responseString);

            return new SkyFile();// skyFile;
        }
    }

    public interface IApiService
    {
        event EventHandler<(long uploaded, long size)> UploadProgressUpdate;

        Task<SkyFile> UploadFile(string filename, byte[] file, long fileSizeBytes);

        Task<SkyFile> UploadFile2(string filename, byte[] file, long fileSizeBytes);
    }
}
