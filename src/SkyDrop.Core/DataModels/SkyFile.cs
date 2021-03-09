using System;
using System.ComponentModel;
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

        //only for staged files
        [Realms.Ignored]
        public byte[] Data { get; set; }

        public FileStatus Status
        {
            get => (FileStatus)statusInt;
            set => statusInt = (int)value;
        }
        private int statusInt = 1;
    }

    public enum FileStatus
    {
        Staged = 1,
        Uploaded = 2
    }
}
