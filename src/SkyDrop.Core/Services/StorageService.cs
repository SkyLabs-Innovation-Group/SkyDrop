using System;
using System.Collections.Generic;
using System.Linq;
using Realms;
using Realms.Exceptions;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.RealmObjects;
using SkyDrop.Core.Utility;

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

        public List<SkyFile> LoadSkyFiles()
        {
            var realmSkyFiles = realm.All<SkyFileRealmObject>().Select(SkyFileFromRealmObject).ToList();
            foreach(var skyFile in realmSkyFiles)
            {
                skyFile.Status = FileStatus.Uploaded;
            }

            return realmSkyFiles;
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

        public List<Contact> LoadContacts()
        {
            try
            {
                var realmObjects = realm.All<ContactRealmObject>().ToArray();
                return ContactsFromRealmObjects(realmObjects);
            }
            catch (InvalidOperationException)
            {
                log.Trace("No contacts found");
                return null;
            }
        }

        public void AddContact(Contact contact)
        {
            var realmObject = ContactToRealmObject(contact);
            realm.Write(() =>
            {
                realm.Add(realmObject);
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

        private ContactRealmObject ContactToRealmObject(Contact contact)
        {
            return new ContactRealmObject { Name = contact.Name, PublicKeySerialized = Util.PublicKeyToBase64String(contact.PublicKey) };
        }

        private List<Contact> ContactsFromRealmObjects(params ContactRealmObject[] contactRealmObjects)
        {
            return contactRealmObjects.Select(c => new Contact { Name = c.Name, PublicKey = Util.Base64StringToPublicKey(c.PublicKeySerialized) }).ToList();
        }
    }

    public interface IStorageService
    {
        List<SkyFile> LoadSkyFiles();

        void SaveSkyFiles(params SkyFile[] skyFile);

        void DeleteSkyFile(SkyFile skyFile);

        UploadAverage GetAverageUploadRate();

        void SetAverageUploadRate(UploadAverage uploadAverage);

        void ClearAllData();

        List<Contact> LoadContacts();

        void AddContact(Contact contact);
    }
}
