using Realms;

namespace SkyDrop.Core.DataModels
{
    /// <summary>
    /// Average Upload Speed, measured in bits/second
    /// </summary>
    public class UploadAverage : RealmObject
    {
        public double Value { get; set; }
        public int DataPointCount { get; set; }
    }
}
