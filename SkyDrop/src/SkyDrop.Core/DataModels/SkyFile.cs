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
    }
}
