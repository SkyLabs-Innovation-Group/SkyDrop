using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Realms;

namespace SkyDrop.Core.DataModels
{
    public class SkyFile : RealmObject, INotifyPropertyChanged
    {
        /// <summary>
        /// If true, scanned SkyFiles use the scanned URL unmodified, if false, scanned skylinks uses the SkyDrop user's selected portal URL.
        /// </summary>
        public static bool UseUploadPortal = false;
        
        [JsonProperty("skylink")]
        public string Skylink { get; set; }

        [JsonProperty("merkelroot")]
        public string Merkelroot { get; set; }

        [JsonProperty("bitfield")]
        public long BitField { get; set; }
        
        [Ignored]
        public SkynetPortal UploadPortal { get; set; }

        public string Filename { get; set; }
        
        public string FullFilePath { get; set; }

        public long FileSizeBytes { get; set; }

        public Stream GetStream()
        {
            if (FullFilePath == null)
                return null;
            
            return File.OpenRead(FullFilePath);
        }

        /// <summary>
        /// Convert raw skylink to full skylink url
        /// </summary>
        public string GetSkylinkUrl()
        {
            var portal = UseUploadPortal ? UploadPortal : SkynetPortal.SelectedPortal;
            
            return $"{portal}/{Skylink}";
        }
        
        private const int SkylinkLength = 46;
        
        /// <summary>
        /// Checks if a URL is a skyfile
        /// </summary>
        public static bool IsSkyfile(string fullSkylinkUrl) 
        {
            if ((fullSkylinkUrl.Split('/').LastOrDefault()?.Length ?? -1) != SkylinkLength)
                return false;

            return true;
        }
        
        public void SetSkynetPortalUploadedTo(SkynetPortal portal)
        {
            if (UploadPortal == null)
                UploadPortal = portal;
        }

        private int statusInt = 1;
        public FileStatus Status
        {
            get => (FileStatus)statusInt;
            set => statusInt = (int)value;
        }

        /// <summary>
        /// True if this file was sent from this device
        /// False if this file was recieved from another device
        /// </summary>
        public bool WasSent { get; set; }
    }

    public enum FileStatus
    {
        Staged = 1,
        Uploaded = 2
    }
}
