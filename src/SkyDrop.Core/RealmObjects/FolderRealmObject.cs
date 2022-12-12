using Realms;

namespace SkyDrop.Core.RealmObjects
{
    public class FolderRealmObject : RealmObject
    {
        [PrimaryKey] public string Id { get; set; }

        public string Name { get; set; }

        public string SkyLinks { get; set; }
    }
}