using System;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using Realms;

namespace SkyDrop.Core.DataModels
{
    public class SkyFile : RealmObject, INotifyPropertyChanged
    {
        [JsonProperty("skylink")]
        public string Skylink { get; set; }

        [JsonProperty("merkelroot")]
        public string Merkelroot { get; set; }

        [JsonProperty("bitfield")]
        public long BitField { get; set; }

        public string Filename { get; set; }
        
        public string FullFilePath { get; set; }

        public long FileSizeBytes { get; set; }

        public Stream GetStream()
        {
            if (FullFilePath == null)
                return null;
            
            return File.OpenRead(FullFilePath);
        }

        private int statusInt = 1;
        public FileStatus Status
        {
            get => (FileStatus)statusInt;
            set => statusInt = (int)value;
        }

        public bool ShouldDownsample => true;
    }

    public enum FileStatus
    {
        Staged = 1,
        Uploaded = 2
    }
}
