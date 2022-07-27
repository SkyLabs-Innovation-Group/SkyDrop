using System;
using System.Collections.Generic;
using System.Linq;
using Realms;
using Realms.Exceptions;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.RealmObjects;

namespace SkyDrop.Core.Services
{
    public class StorageService : IStorageService
    {
        private readonly ILog log;
        private Realm realm => GetRealm();

        public StorageService(ILog log)
        {
            this.log = log;
        }
        /*
        public List<SkyFile> LoadAllSkyFiles()
        {
            var realmSkyFiles = realm.All<SkyFileRealmObject>().Select(SkyFileFromRealmObject).ToList();
            foreach(var skyFile in realmSkyFiles)
            {
                skyFile.Status = FileStatus.Uploaded;
            }

            return realmSkyFiles;
        }
        */
        public List<SkyFile> LoadSentSkyFiles()
        {
            var realmSkyFiles = realm.All<SkyFileRealmObject>().Where(f => f.WasSent);
            return realmSkyFiles.Select(SkyFileFromRealmObject).ToList();
        }

        public List<SkyFile> LoadReceivedSkyFiles()
        {
            var realmSkyFiles = realm.All<SkyFileRealmObject>().Where(f => !f.WasSent);
            return realmSkyFiles.Select(SkyFileFromRealmObject).ToList();
        }

        public List<SkyFile> LoadSkyFilesWithSkylinks(List<string> skylinks)
        {
            var realmSkyFiles = realm.All<SkyFileRealmObject>().Where(a => skylinks.Contains(a.Skylink)).Select(SkyFileFromRealmObject).ToList();
            foreach (var skyFile in realmSkyFiles)
            {
                skyFile.Status = FileStatus.Uploaded;
            }

            return realmSkyFiles;
        }

        public List<Folder> LoadFolders()
        {
            var folders = realm.All<FolderRealmObject>();
            return folders.Select(FolderFromRealmObject).ToList();
        }

        public void SaveFolder(Folder folder)
        {
            var realmObject = FolderToRealmObject(folder);
            realm.Write(() =>
            {
                realm.Add(realmObject, true);
            });
        }

        int saveCallCount = 0;

        public void SaveSkyFiles(params SkyFile[] skyFiles)
        {
            saveCallCount++;

            realm.Write(() =>
            {
                log.Trace("SaveSkyFiles() write for call #" + saveCallCount);
                realm.Add(skyFiles.Select(SkyFileToRealmObject), true);
            });
        }

        public void DeleteSkyFile(SkyFile skyFile)
        {
            realm.Write(() =>
            {
                var realmObject = realm.Find<SkyFileRealmObject>(skyFile.Skylink);
                realm.Remove(realmObject);
            });
        }

        public UploadAverage GetAverageUploadRate()
        {
            try
            {
                var uploadAverage = realm.All<UploadAverage>().First();
                return uploadAverage;
            }
            catch (InvalidOperationException)
            {
                log.Trace("No uploaded file in UploadAverage table");
                return new UploadAverage();
            }
        }

        public void SetAverageUploadRate(UploadAverage uploadAverage)
        {
            realm.Write(() =>
            {
                log.Trace("SetAverageUploadRate()");
                realm.RemoveAll<UploadAverage>();
                realm.Add(uploadAverage);
            });
        }

        public void ClearAllData()
        {
            realm.Write(() =>
            {
                realm.RemoveAll();
            });
        }

        /// <summary>
        /// If there is a data discrepancy,
        /// Clears the whole database off and resets
        /// </summary>
        private Realm GetRealm()
        {
            var realmConfiguration = new RealmConfiguration();

            try
            {
                return Realm.GetInstance(realmConfiguration);
            }
            catch (RealmException e)
            {
                try
                {
                    log.Exception(e);
                    Realm.DeleteRealm(realmConfiguration);

                    //Realm file has been deleted.
                    return Realm.GetInstance(realmConfiguration);
                }
                catch (Exception ex)
                {
                    //No Realm file to remove.
                    log.Exception(e);
                    throw ex;
                }
            }
        }

        private SkyFileRealmObject SkyFileToRealmObject(SkyFile skyFile)
        {
            return new SkyFileRealmObject { Filename = skyFile.Filename, Skylink = skyFile.Skylink, WasSent = skyFile.WasSent };
        }

        private SkyFile SkyFileFromRealmObject(SkyFileRealmObject realmObject)
        {
            return new SkyFile { Filename = realmObject.Filename, Skylink = realmObject.Skylink, WasSent = realmObject.WasSent };
        }

        private Folder FolderFromRealmObject(FolderRealmObject realmObject)
        {
            return new Folder { Id = new Guid(realmObject.Id), Name = realmObject.Name, SkyLinks = realmObject.SkyLinks.Split(',').ToList() };
        }

        private FolderRealmObject FolderToRealmObject(Folder folder)
        {
            return new FolderRealmObject { Id = folder.Id.ToString(), Name = folder.Name, SkyLinks = string.Join(",", folder.SkyLinks) };
        }
    }

    public interface IStorageService
    {
        //List<SkyFile> LoadAllSkyFiles();

        List<SkyFile> LoadSkyFilesWithSkylinks(List<string> skylinks);

        List<SkyFile> LoadSentSkyFiles();

        List<SkyFile> LoadReceivedSkyFiles();

        List<Folder> LoadFolders();

        void SaveFolder(Folder folder);

        void SaveSkyFiles(params SkyFile[] skyFile);

        void DeleteSkyFile(SkyFile skyFile);

        UploadAverage GetAverageUploadRate();

        void SetAverageUploadRate(UploadAverage uploadAverage);

        void ClearAllData();
    }
}
