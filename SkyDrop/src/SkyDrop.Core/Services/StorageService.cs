using System.Collections.Generic;
using System.Linq;
using Realms;
using SkyDrop.Core.DataModels;

namespace SkyDrop.Core.Services
{
    public class StorageService : IStorageService
    {
        private readonly ILog log;

        public StorageService(ILog log)
        {
            this.log = log;
        }

        public List<SkyFile> LoadSkyFiles()
        {
            var realm = Realm.GetInstance();

            var realmSkyFiles = realm.All<SkyFile>().ToList();

            return realmSkyFiles;
        }

        int saveCallCount = 0;

        public void SaveSkyFiles(params SkyFile[] skyFiles)
        {
            saveCallCount++;

            var realm = Realm.GetInstance();

            realm.Write(() =>
            {
                log.Trace("SaveSkyFiles() write for call #" + saveCallCount);
                realm.Add(skyFiles);
            });
        }

        public void DeleteSkyFile(SkyFile skyFile)
        {
            var realm = Realm.GetInstance();

            realm.Write(() =>
            {
                realm.Remove(skyFile);
            });
        }
    }

    public interface IStorageService
    {
        List<SkyFile> LoadSkyFiles();

        void SaveSkyFiles(params SkyFile[] skyFile);

        void DeleteSkyFile(SkyFile skyFile);
    }
}
