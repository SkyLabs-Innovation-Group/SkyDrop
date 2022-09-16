using System;
using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.Crypto.Parameters;
using Realms;
using Realms.Exceptions;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
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
            var realmSkyFiles = realm.All<SkyFileRealmObject>().ToList() //additional ToList() fixes "Contains not supported" error
                .Where(a => skylinks.Contains(a.Skylink)).Select(SkyFileFromRealmObject).ToList();

            foreach (var skyFile in realmSkyFiles)
            {
                skyFile.Status = FileStatus.Uploaded;
            }

            return realmSkyFiles;
        }

        public List<Folder> LoadFolders()
        {
            var folders = realm.All<FolderRealmObject>();
            return folders.Select(FolderFromRealmObject).OrderBy(f => f.Name).ToList();
        }

        public void SaveFolder(Folder folder)
        {
            var realmObject = FolderToRealmObject(folder);
            realm.Write(() =>
            {
                realm.Add(realmObject, true);
            });
        }

        public void DeleteFolder(Folder folder)
        {
            realm.Write(() =>
            {
                var realmObject = realm.Find<FolderRealmObject>(folder.Id.ToString());
                realm.Remove(realmObject);
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

        public void DeleteSkyFile(SkyFile skyFile, Folder folder)
        {
            if (folder == null)
            {
                var folders = realm.All<FolderRealmObject>().ToList() //additional ToList() fixes "Contains not supported" error
                    .Where(f => f.SkyLinks.Contains(skyFile.Skylink)).ToList();

                realm.Write(() =>
                {
                    //remove the skylink from all folders
                    foreach (var oldFolderObj in folders)
                    {
                        var oldFolder = FolderFromRealmObject(oldFolderObj);
                        oldFolder.SkyLinks = oldFolder.SkyLinks.Where(s => s != skyFile.Skylink).ToList();
                        var newFolderObj = FolderToRealmObject(oldFolder);
                        realm.Remove(oldFolderObj);
                        realm.Add(newFolderObj);
                    }

                    //delete SkyFile record
                    var fileObject = realm.Find<SkyFileRealmObject>(skyFile.Skylink);
                    realm.Remove(fileObject);
                });

                return;
            }

            //remove the file from a custom folder

            folder.SkyLinks = folder.SkyLinks.Where(s => s != skyFile.Skylink).ToList();
            var newFolderObject = FolderToRealmObject(folder);

            realm.Write(() =>
            {
                var oldFolderObject = realm.Find<FolderRealmObject>(folder.Id.ToString());
                realm.Remove(oldFolderObject);

                realm.Add(newFolderObject);
            });
        }

        public void MoveSkyFiles(List<SkyFile> skyFiles, Folder folder)
        {
            folder.SkyLinks.AddRange(skyFiles.Select(s => s.Skylink));
            var newFolderObject = FolderToRealmObject(folder);

            realm.Write(() =>
            {
                var oldFolderObject = realm.Find<FolderRealmObject>(folder.Id.ToString());
                realm.Remove(oldFolderObject);

                realm.Add(newFolderObject);
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
            var (exists, name) = ContactExists(contact.Id);
            if (exists)
                throw new Exception($"Contact already saved as {name}");

            realm.Write(() =>
            {
                var realmObject = ContactToRealmObject(contact);
                realm.Add(realmObject);
            });
        }

        public (bool exists, string savedName) ContactExists(Guid id)
        {
            var existingContact = realm.Find<ContactRealmObject>(id.ToString());
            var exists = existingContact != null;
            return (exists, existingContact?.Name);
        }

        public void DeleteContact(Contact contact)
        {
            realm.Write(() =>
            {
                var realmObject = realm.Find<ContactRealmObject>(contact.Id.ToString());
                realm.Remove(realmObject);
            });
        }

        public EncryptionKeyPairRealmObject GetMyEncryptionKeys()
        {
            return realm.All<EncryptionKeyPairRealmObject>().FirstOrDefault();
        }

        public void SaveMyEncryptionKeys(X25519PrivateKeyParameters privateKey, X25519PublicKeyParameters publicKey, Guid id, string deviceName)
        {
            var privateBytes = privateKey.GetEncoded();
            var privateKeyString = Convert.ToBase64String(privateBytes);

            var publicBytes = publicKey.GetEncoded();
            var publicKeyString = Convert.ToBase64String(publicBytes);

            var keys = new EncryptionKeyPairRealmObject()
            {
                PrivateKeyBase64 = privateKeyString,
                PublicKeyBase64 = publicKeyString,
                Id = id.ToString(),
                Name = deviceName
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
        
        private Folder FolderFromRealmObject(FolderRealmObject realmObject)
        {
            return new Folder { Id = new Guid(realmObject.Id), Name = realmObject.Name, SkyLinks = realmObject.SkyLinks.IsNullOrEmpty() ? new List<string>() : realmObject.SkyLinks.Split(',').ToList()  };
        }

        private FolderRealmObject FolderToRealmObject(Folder folder)
        {
            return new FolderRealmObject { Id = folder.Id.ToString(), Name = folder.Name, SkyLinks = string.Join(",", folder.SkyLinks) };
        }
    }

    public interface IStorageService
    {
        List<SkyFile> LoadSkyFilesWithSkylinks(List<string> skylinks);

        List<SkyFile> LoadSentSkyFiles();

        List<SkyFile> LoadReceivedSkyFiles();

        List<Folder> LoadFolders();

        void SaveFolder(Folder folder);

        void DeleteFolder(Folder folder);

        void SaveSkyFiles(params SkyFile[] skyFile);

        void DeleteSkyFile(SkyFile skyFile, Folder folder);

        void MoveSkyFiles(List<SkyFile> skyFiles, Folder folder);

        UploadAverage GetAverageUploadRate();

        void SetAverageUploadRate(UploadAverage uploadAverage);

        void ClearAllData();

        List<Contact> LoadContacts();

        void AddContact(Contact contact);

        (bool exists, string savedName) ContactExists(Guid publicKeyBase64);

        void DeleteContact(Contact contact);

        void SaveMyEncryptionKeys(X25519PrivateKeyParameters privateKey, X25519PublicKeyParameters publicKey, Guid id, string deviceName);

        EncryptionKeyPairRealmObject GetMyEncryptionKeys();
    }
}
