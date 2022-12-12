using Realms;

namespace SkyDrop.Core.RealmObjects
{
    public class SkyFileRealmObject : RealmObject
    {
        [PrimaryKey] public string Skylink { get; set; }

        public string Filename { get; set; }

        public bool WasSent { get; set; }
    }
}