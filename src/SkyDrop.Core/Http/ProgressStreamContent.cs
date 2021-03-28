using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkyDrop.Core.Http
{
    /// <summary>
    /// Tracks upload progress of a file in a post request
    /// Adapted from this answer
    /// https://stackoverflow.com/a/41392145/9636501
    /// </summary>
    public class ProgressStreamContent : HttpContent
    {
        public event EventHandler<(long uploaded, long size)> ProgressUpdate;

        private const int defaultBufferSize = 5 * 4096; //buffer of 20kb
        private HttpContent content;
        private int bufferSize;

        public ProgressStreamContent(HttpContent content) : this(content, defaultBufferSize) { }

        public ProgressStreamContent(HttpContent content, int bufferSize)
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

            foreach (var h in content.Headers)
            {
                this.Headers.Add(h.Key, h.Value);
            }
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return Task.Run(async () =>
            {
                var buffer = new byte[this.bufferSize];
                long size;
                TryComputeLength(out size);
                var uploaded = 0;

                using (var sinput = await content.ReadAsStreamAsync())
                {
                    while (true)
                    {
                        var length = sinput.Read(buffer, 0, buffer.Length);
                        if (length <= 0) break;

                        uploaded += length;
                        ProgressUpdate?.Invoke(this, (uploaded, size));

                        System.Diagnostics.Debug.WriteLine($"Bytes sent {uploaded} of {size}");

                        stream.Write(buffer, 0, length);
                        stream.Flush();
                    }
                }

                stream.Flush();
            });
        }

        protected override bool TryComputeLength(out long length)
        {
            length = content.Headers.ContentLength.GetValueOrDefault();
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
    }
}
