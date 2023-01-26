using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.ViewModels;
using Org.BouncyCastle.Crypto.Parameters;
using Realms;
using Realms.Exceptions;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.RealmObjects;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using Xamarin.Essentials;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Contact = SkyDrop.Core.DataModels.Contact;

namespace SkyDrop.Core.Services
{
    public class StorageService : IStorageService
    {
        private const string PrivateKeyStorageKey = "private_key";
        private const string PublicKeyStorageKey = "public_key";
        private const string IdStorageKey = "device_id";
        private const string NameStorageKey = "device_name";
        private readonly ILog log;

        private int saveCallCount;

        public StorageService(ILog log)
        {
            this.log = log;
        }

        private Realm Realm => GetRealm();

        public List<SkyFile> LoadSentSkyFiles()
        {
            var realmSkyFiles = Realm.All<SkyFileRealmObject>().Where(f => f.WasSent);
            return realmSkyFiles.Select(SkyFileFromRealmObject).ToList();
        }

        public List<SkyFile> LoadReceivedSkyFiles()
        {
            var realmSkyFiles = Realm.All<SkyFileRealmObject>().Where(f => !f.WasSent);
            return realmSkyFiles.Select(SkyFileFromRealmObject).ToList();
        }

        public List<SkyFile> LoadSkyFilesWithSkylinks(List<string> skylinks)
        {
            var realmSkyFiles = Realm.All<SkyFileRealmObject>()
                .ToList() //additional ToList() fixes "Contains not supported" error
                .Where(a => skylinks.Contains(a.Skylink)).Select(SkyFileFromRealmObject).ToList();

            foreach (var skyFile in realmSkyFiles) skyFile.Status = FileStatus.Uploaded;

            return realmSkyFiles;
        }

        public List<Folder> LoadFolders()
        {
            var folders = Realm.All<FolderRealmObject>();
            return folders.Select(FolderFromRealmObject).OrderBy(f => f.Name).ToList();
        }

        public void SaveFolder(Folder folder)
        {
            var realmObject = FolderToRealmObject(folder);
            Realm.Write(() => { Realm.Add(realmObject, true); });
        }

        public void DeleteFolder(Folder folder)
        {
            Realm.Write(() =>
            {
                var realmObject = Realm.Find<FolderRealmObject>(folder.Id.ToString());
                Realm.Remove(realmObject);
            });
        }

        public void SaveSkyFiles(params SkyFile[] skyFiles)
        {
            saveCallCount++;

            Realm.Write(() =>
            {
                log.Trace("SaveSkyFiles() write for call #" + saveCallCount);
                Realm.Add(skyFiles.Select(SkyFileToRealmObject), true);
            });
        }

        public void DeleteSkyFile(SkyFile skyFile, Folder folder)
        {
            if (folder == null)
            {
                var folders = Realm.All<FolderRealmObject>()
                    .ToList() //additional ToList() fixes "Contains not supported" error
                    .Where(f => f.SkyLinks.Contains(skyFile.Skylink)).ToList();

                Realm.Write(() =>
                {
                    //remove the skylink from all folders
                    foreach (var oldFolderObj in folders)
                    {
                        var oldFolder = FolderFromRealmObject(oldFolderObj);
                        oldFolder.SkyLinks = oldFolder.SkyLinks.Where(s => s != skyFile.Skylink).ToList();
                        var newFolderObj = FolderToRealmObject(oldFolder);
                        Realm.Remove(oldFolderObj);
                        Realm.Add(newFolderObj);
                    }

                    //delete SkyFile record
                    var fileObject = Realm.Find<SkyFileRealmObject>(skyFile.Skylink);
                    Realm.Remove(fileObject);
                });

                return;
            }

            //remove the file from a custom folder

            folder.SkyLinks = folder.SkyLinks.Where(s => s != skyFile.Skylink).ToList();
            var newFolderObject = FolderToRealmObject(folder);

            Realm.Write(() =>
            {
                var oldFolderObject = Realm.Find<FolderRealmObject>(folder.Id.ToString());
                Realm.Remove(oldFolderObject);

                Realm.Add(newFolderObject);
            });
        }

        public void MoveSkyFiles(List<SkyFile> skyFiles, Folder folder)
        {
            folder.SkyLinks.AddRange(skyFiles.Select(s => s.Skylink));
            var newFolderObject = FolderToRealmObject(folder);

            Realm.Write(() =>
            {
                var oldFolderObject = Realm.Find<FolderRealmObject>(folder.Id.ToString());
                Realm.Remove(oldFolderObject);

                Realm.Add(newFolderObject);
            });
        }

        public UploadAverage GetAverageUploadRate()
        {
            try
            {
                var uploadAverage = Realm.All<UploadAverage>().First();
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
            Realm.Write(() =>
            {
                log.Trace("SetAverageUploadRate()");
                Realm.RemoveAll<UploadAverage>();
                Realm.Add(uploadAverage);
            });
        }

        public List<Contact> LoadContacts()
        {
            try
            {
                var realmObjects = Realm.All<ContactRealmObject>().ToArray();
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

            //gracefully avoid naming collisions by adding (1), (2), (3) etc. to the end of the contact name
            var i = 1;
            while (ContactNameExists(contact.Name))
            {
                var formattedContactName = contact.Name + " {0}";
                contact.Name = string.Format(formattedContactName, "(" + i++ + ")");
            }

            Realm.Write(() =>
            {
                var realmObject = ContactToRealmObject(contact);
                Realm.Add(realmObject);
            });
        }

        public (bool exists, string savedName) ContactExists(Guid id)
        {
            var existingContact = Realm.Find<ContactRealmObject>(id.ToString());
            var exists = existingContact != null;
            return (exists, existingContact?.Name);
        }

        public void DeleteContact(Contact contact)
        {
            Realm.Write(() =>
            {
                var realmObject = Realm.Find<ContactRealmObject>(contact.Id.ToString());
                Realm.Remove(realmObject);
            });
        }

        public async Task UpdateDeviceName(string name)
        {
            await SecureStorage.SetAsync(NameStorageKey, name);
        }

        public void RenameContact(Contact contact, string newName)
        {
            contact.Name = newName;
            Realm.Write(() =>
            {
                var realmObj = Realm.Find<ContactRealmObject>(contact.Id.ToString());
                realmObj.Name = newName;
                Realm.Add(realmObj);
            });
        }

        public async Task<EncryptionKeys> GetMyEncryptionKeys()
        {
            var id = await SecureStorage.GetAsync(IdStorageKey);
            if (id == null)
                return null;

            return new EncryptionKeys
            {
                Id = id,
                PublicKeyBase64 = await SecureStorage.GetAsync(PublicKeyStorageKey),
                PrivateKeyBase64 = await SecureStorage.GetAsync(PrivateKeyStorageKey),
                Name = await SecureStorage.GetAsync(NameStorageKey)
            };
        }

        public async Task SaveMyEncryptionKeys(X25519PrivateKeyParameters privateKey,
            X25519PublicKeyParameters publicKey, Guid id, string deviceName)
        {
            var privateBytes = privateKey.GetEncoded();
            var privateKeyString = Convert.ToBase64String(privateBytes);

            var publicBytes = publicKey.GetEncoded();
            var publicKeyString = Convert.ToBase64String(publicBytes);

            await SecureStorage.SetAsync(PublicKeyStorageKey, publicKeyString);
            await SecureStorage.SetAsync(PrivateKeyStorageKey, privateKeyString);
            await SecureStorage.SetAsync(IdStorageKey, id.ToString());
            await SecureStorage.SetAsync(NameStorageKey, deviceName);
        }


        public void ClearAllData()
        {
            Realm.Write(() => { Realm.RemoveAll(); });
        }

        public void DeleteSkynetPortal(SkynetPortal portal)
        {
            Realm.Write(() => { Realm.Remove(portal); });
        }

        public async Task SaveSkynetPortal(SkynetPortal portal, string apiToken = null)
        {
            Realm.Write(() =>
            {
                Realm.Add(portal);
            });

            portal.UserApiToken ??= apiToken;
            await SaveApiTokenToSecureStorage(portal.GetApiTokenPrefKey(), apiToken);
        }

        public async Task EditSkynetPortal(SkynetPortal portal, string newPortalName, string newPortalUrl, string apiToken = null)
        {
            if (portal == null)
                throw new ArgumentNullException(nameof(portal));

            portal.UserApiToken ??= apiToken;
            await SaveApiTokenToSecureStorage(portal.GetApiTokenPrefKey(), apiToken);

            Realm.Write(() =>
            {
                var storedPortal = Realm.Find<SkynetPortal>(portal.Id);

                storedPortal.Name = newPortalName;
                storedPortal.BaseUrl = newPortalUrl;
                storedPortal.PortalPreferencesPosition = portal.PortalPreferencesPosition;

                Realm.Add(storedPortal);
            });
        }


        public void SetSkynetPortalLoggedInBrowser(SkynetPortal portal)
        {
            Realm.Write(() =>
            {
                var storedPortal = Realm.Find<SkynetPortal>(portal.Id);
                storedPortal.HasLoggedInBrowser = true;

                Realm.Add(storedPortal);
            });
        }

        public List<SkynetPortal> LoadSkynetPortals()
        {
            return Realm.All<SkynetPortal>().OrderBy(portal => portal.PortalPreferencesPosition).ToList();
        }

        public SkynetPortal LoadSkynetPortal(string portalId)
        {
            return Realm.Find<SkynetPortal>(portalId);
        }

        private bool ContactNameExists(string contactName)
        {
            var existingContact = Realm.All<ContactRealmObject>().ToList().FirstOrDefault(c => c.Name == contactName);
            var exists = existingContact != null;
            return exists;
        }

        /// <summary>
        /// If there is a data discrepancy,
        /// Clears the whole database off and resets
        /// </summary>
        private Realm GetRealm()
        {
            const ulong currentSchemaVersion = 1;

            var realmConfiguration = new RealmConfiguration()
            {
                SchemaVersion = currentSchemaVersion
            };

            try
            {

                realmConfiguration.MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    if (oldSchemaVersion < 1)
                    {
                        //var newPortals = migration.NewRealm.All<SkynetPortal>();
                        //var oldPortals = migration.OldRealm.All<SkynetPortal>();

                        // No migration needed
                    }
                };

                var instance = Realm.GetInstance(realmConfiguration);

                return instance;
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
            return new SkyFileRealmObject
            {
                Filename = skyFile.Filename,
                Skylink = skyFile.Skylink,
                WasSent = skyFile.WasSent
            };
        }

        private SkyFile SkyFileFromRealmObject(SkyFileRealmObject realmObject)
        {
            return new SkyFile
            {
                Filename = realmObject.Filename,
                Skylink = realmObject.Skylink,
                WasSent = realmObject.WasSent
            };
        }

        private ContactRealmObject ContactToRealmObject(Contact contact)
        {
            return new ContactRealmObject
            {
                Id = contact.Id.ToString(),
                Name = contact.Name,
                PublicKeyBase64 = contact.PublicKey.PublicKeyToBase64String()
            };
        }

        private List<Contact> ContactsFromRealmObjects(params ContactRealmObject[] contactRealmObjects)
        {
            return contactRealmObjects.Select(c => new Contact
            {
                Id = new Guid(c.Id),
                Name = c.Name,
                PublicKey = c.PublicKeyBase64.Base64StringToPublicKey()
            }).ToList();
        }

        private Folder FolderFromRealmObject(FolderRealmObject realmObject)
        {
            return new Folder
            {
                Id = new Guid(realmObject.Id),
                Name = realmObject.Name,
                SkyLinks = realmObject.SkyLinks.IsNullOrEmpty()
                    ? new List<string>()
                    : realmObject.SkyLinks.Split(',').ToList()
            };
        }

        private FolderRealmObject FolderToRealmObject(Folder folder)
        {
            return new FolderRealmObject
            {
                Id = folder.Id.ToString(),
                Name = folder.Name,
                SkyLinks = string.Join(",", folder.SkyLinks)
            };
        }

        private Task SaveApiTokenToSecureStorage(string portalPrefKey, string apiToken)
        {
            if (string.IsNullOrEmpty(apiToken))
                return Task.CompletedTask;

            return SecureStorage.SetAsync(portalPrefKey, apiToken);
        }

        public void ReorderPortals(SkynetPortal portal, int oldPosition, int newPosition)
        {
            Realm.Write(() =>
            {
                var portals = LoadSkynetPortals();

                if (oldPosition < 0 || oldPosition >= portals.Count || newPosition < 0 || newPosition >= portals.Count)
                    return;

                var temp = portals[oldPosition];
                portals[oldPosition] = portals[newPosition];
                portals[newPosition] = temp;

                portals[oldPosition].PortalPreferencesPosition = oldPosition;
                portals[newPosition].PortalPreferencesPosition = newPosition;

                Realm.Add(portals[oldPosition]);
                Realm.Add(portals[newPosition]);
            });
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

        Task SaveMyEncryptionKeys(X25519PrivateKeyParameters privateKey, X25519PublicKeyParameters publicKey, Guid id,
            string deviceName);

        Task<EncryptionKeys> GetMyEncryptionKeys();

        Task UpdateDeviceName(string name);

        void RenameContact(Contact contact, string newName);

        Task SaveSkynetPortal(SkynetPortal portal, string apiToken = null);

        Task EditSkynetPortal(SkynetPortal portal, string newPortalName, string newPortalUrl, string apiToken = null);

        List<SkynetPortal> LoadSkynetPortals();

        SkynetPortal LoadSkynetPortal(string portalId);

        void DeleteSkynetPortal(SkynetPortal portal);

        void SetSkynetPortalLoggedInBrowser(SkynetPortal portal);

        void ReorderPortals(SkynetPortal portal, int oldPosition, int newPosition);

    }
}