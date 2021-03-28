using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkyDrop.Core.Http
{
    /// <summary>
    /// Tracks upload progress of a file in a post request
    /// Adapted from this answer
    /// https://stackoverflow.com/a/35340010/9636501
    /// </summary>
    public class ProgressableStreamContent : MultipartFormDataContent
    {
        private const int defaultBufferSize = 4096;

        private Stream content;
        private int bufferSize;
        private bool contentConsumed;
        //private UploadProgressTracker downloader;

        public event EventHandler<ProgressEventArgs> ProgressUpdate;
        /*
        public ProgressableStreamContent(Stream content) : base()
        {
        }
        */
        
        public ProgressableStreamContent(Stream content, int bufferSize)//, UploadProgressTracker downloader)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize");
            }

            this.content = content;
            this.bufferSize = bufferSize;
            //this.downloader = downloader;
        }
        
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            Contract.Assert(stream != null);

            PrepareContent();

            return Task.Run(() =>
            {
                var buffer = new byte[this.bufferSize];
                var size = content.Length;
                var uploaded = 0;

                ProgressUpdate?.Invoke(this, new ProgressEventArgs { Progress = 0, UploadState = UploadProgressState.PendingUpload });

                using (content) while (true)
                {
                    var length = content.Read(buffer, 0, buffer.Length);
                    if (length <= 0) break;

                    uploaded = uploaded += length;

                    stream.Write(buffer, 0, length);

                    ProgressUpdate?.Invoke(this, new ProgressEventArgs { Progress = uploaded / size, UploadState = UploadProgressState.Uploading });
                }

                ProgressUpdate?.Invoke(this, new ProgressEventArgs { Progress = -1, UploadState = UploadProgressState.PendingResponse });
            });
        }

        protected override bool TryComputeLength(out long length)
        {
            length = content.Length;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                content.Dispose();
            }
            base.Dispose(disposing);
        }

        private void PrepareContent()
        {
            if (contentConsumed)
            {
                // If the content needs to be written to a target stream a 2nd time, then the stream must support
                // seeking (e.g. a FileStream), otherwise the stream can't be copied a second time to a target 
                // stream (e.g. a NetworkStream).
                if (content.CanSeek)
                {
                    content.Position = 0;
                }
                else
                {
                    throw new InvalidOperationException("SR.net_http_content_stream_already_read");
                }
            }

            contentConsumed = true;
        }
    }

    public class ProgressEventArgs
    {
        public double Progress;
        public UploadProgressState UploadState;
    }

    public enum UploadProgressState
    {
        PendingUpload = 2,
        Uploading = 3,
        PendingResponse = 4
    }
}
