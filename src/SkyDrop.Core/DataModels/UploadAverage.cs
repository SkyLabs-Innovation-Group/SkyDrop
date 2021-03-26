using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Realms;

namespace SkyDrop.Core.DataModels
{
    /// <summary>
    /// Average Upload Speed, measured in bits/second
    /// </summary>
    public class UploadAverage : RealmObject, INotifyPropertyChanged
    {
        public double Value { get; set; }
        public int DataPointCount { get; set; }
    }
}
