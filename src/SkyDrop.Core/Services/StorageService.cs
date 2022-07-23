using System;
using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.Crypto.Parameters;
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
            if (ContactExists(contact.Id))
                throw new Exception("Contact already exists");

            realm.Write(() =>
            {
                var realmObject = ContactToRealmObject(contact);
                realm.Add(realmObject);
            });
        }

        public bool ContactExists(Guid id)
        {
            return realm.Find<ContactRealmObject>(id.ToString()) != null;
        }

        public EncryptionKeyPairRealmObject GetMyEncryptionKeys()
        {
            return realm.All<EncryptionKeyPairRealmObject>().FirstOrDefault();
        }

        public void SaveMyEncryptionKeys(X25519PrivateKeyParameters privateKey, X25519PublicKeyParameters publicKey, Guid id)
        {
            var privateBytes = privateKey.GetEncoded();
            var privateKeyString = Convert.ToBase64String(privateBytes);

            var publicBytes = publicKey.GetEncoded();
            var publicKeyString = Convert.ToBase64String(publicBytes);

            var keys = new EncryptionKeyPairRealmObject()
            {
                PrivateKeyBase64 = privateKeyString,
                PublicKeyBase64 = publicKeyString,
                Id = id.ToString()
            };

            realm.Write(() =>
            {
                realm.RemoveAll<EncryptionKeyPairRealmObject>();
                realm.Add(keys);
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
            return new ContactRealmObject { Id = contact.Id.ToString(), Name = contact.Name, PublicKeyBase64 = Util.PublicKeyToBase64String(contact.PublicKey) };
        }

        private List<Contact> ContactsFromRealmObjects(params ContactRealmObject[] contactRealmObjects)
        {
            return contactRealmObjects.Select(c => new Contact { Id = new Guid(c.Id), Name = c.Name, PublicKey = Util.Base64StringToPublicKey(c.PublicKeyBase64) }).ToList();
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

        bool ContactExists(Guid publicKeyBase64);

        void SaveMyEncryptionKeys(X25519PrivateKeyParameters privateKey, X25519PublicKeyParameters publicKey, Guid id);

        EncryptionKeyPairRealmObject GetMyEncryptionKeys();
    }
}
