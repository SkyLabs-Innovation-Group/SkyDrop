using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;
using static SkyDrop.Core.Services.EncryptionService;

namespace SkyDrop.Core.Services
{
    /// <summary>
    /// A public key QR code contains this data format:
    /// myId 16 bytes
    /// justScannedId 16 bytes
    /// publicKey 32 bytes
    ///
    /// An encrypted (.skydrop) file contains this data format:
    /// senderId 16 bytes
    /// encryptedData ? bytes
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        public enum AddContactResult
        {
            Default,
            AlreadyExists,
            InvalidKey,
            WrongDevice,
            ContactAdded,
            DevicesPaired
        }

        private IBlockCipher engine = new ThreefishEngine(256); //the cipher engine for encryption
        private IAsymmetricCipherKeyPairGenerator keyGen = new X25519KeyPairGenerator(); //keypair generator for X25519 key agreement scheme
        private X25519PrivateKeyParameters myPrivateKey;
        private X25519PublicKeyParameters myPublicKey;
        private Guid myId;
        private readonly SecureRandom random = new SecureRandom();
        private const int guidSizeBytes = 16;

        private readonly IUserDialogs userDialogs;
        private readonly IStorageService storageService;
        private readonly IFileSystemService fileSystemService;

        public EncryptionService(IUserDialogs userDialogs,
                                 IStorageService storageService,
                                 IFileSystemService fileSystemService)
        {
            this.userDialogs = userDialogs;
            this.storageService = storageService;
            this.fileSystemService = fileSystemService;

            try
            {
                GetKeys();
            }
            catch(Exception e)
            {
                var message = e.Message;
                userDialogs.Toast(message);
            }
        }

        public string GetMyPublicKeyWithId(Guid justScannedId)
        {
            var myIdBytes = myId.ToByteArray();
            var justScannedIdBytes = justScannedId.ToByteArray();
            var keyBytes = myPublicKey.GetEncoded();
            var publicKeyWithId = Util.Combine(myIdBytes, justScannedIdBytes, keyBytes);
            return Convert.ToBase64String(publicKeyWithId);
        }

        public Task<string> EncodeFileFor(string filePath, Contact recipient)
        {
            return Task.Run(() =>
            {
                //get shared secret
                var sharedSecret = GetSharedSecret(myPrivateKey, recipient.PublicKey);

                //load the file
                var fileBytes = File.ReadAllBytes(filePath);

                //encrypt the file
                var encryptedBytes = Encrypt(sharedSecret, fileBytes);

                //add the metadata
                var metaData = GetMetaDataForFile();
                var encryptedFileWithMetaData = Util.Combine(metaData, encryptedBytes);

                //save the file
                var encryptedFilePath = Path.Combine(fileSystemService.CacheFolderPath, $"{Path.GetFileName(filePath)}.skydrop");
                File.WriteAllBytes(encryptedFilePath, encryptedFileWithMetaData);

                return encryptedFilePath;
            });
        }

        public Task<string> DecodeFile(string filePath)
        {
            return Task.Run(() =>
            {
                if (!filePath.EndsWith(".skydrop"))
                    throw new Exception("File must have .skydrop extension");

                //load the file
                if (!File.Exists(filePath))
                    throw new Exception("File does not exist");
                    
                var fileBytes = File.ReadAllBytes(filePath);

                //separate encryptedData and metaData
                var (metaData, encryptedData) = ReadMetaDataFromFile(fileBytes);

                var senderId = new Guid(metaData);
                if (senderId == myId)
                    throw new Exception("Sent files can only be decrypted by their recipients");

                var sender = GetContactWithId(senderId);
                if (sender == null)
                    throw new Exception("SkyFile sender not in contacts list");

                //get shared secret
                var sharedSecret = GetSharedSecret(myPrivateKey, sender.PublicKey);

                //decrypt the file
                var decryptedBytes = Decrypt(sharedSecret, encryptedData);

                //remove ".skydrop" from the end of the filename
                var fileName = Path.GetFileName(filePath);
                fileName = fileName.Substring(0, fileName.Length - 8);

                //save the file
                var decryptedFilePath = Path.Combine(fileSystemService.DownloadsFolderPath, fileName);
                File.WriteAllBytes(decryptedFilePath, decryptedBytes);

                return decryptedFilePath;
            });
        }

        public (AddContactResult result, Guid newContactId) AddPublicKey(string publicKeyEncoded, string contactName)
        {
            var (publicKey, keyId, justScannedId) = DecodePublicKey(publicKeyEncoded);
            if (publicKey == null)
            {
                userDialogs.Alert("Invalid key");
                return (AddContactResult.InvalidKey, default);
            }

            if (storageService.ContactExists(keyId))
            {
                userDialogs.Alert("Contact already exists");
                return (AddContactResult.AlreadyExists, default);
            }

            if (justScannedId != default && justScannedId != myId)
            {
                //you scanned the QR code with the wrong phone
                userDialogs.Alert("Unexpected device! Please go back and try to pair again");
                return (AddContactResult.WrongDevice, default);
            }

            var newContact = new Contact { Name = contactName.Trim(), PublicKey = publicKey, Id = keyId };
            storageService.AddContact(newContact);

            bool didPair = justScannedId == myId; //if justScannedId == default, then we still need to scan this device's QR code to pair
            string message = didPair ? $"Paired with {newContact.Name} successfully" : $"{newContact.Name} added, now scan this QR code on the other device";
            var addContactResult = didPair ? AddContactResult.DevicesPaired : AddContactResult.ContactAdded;

            userDialogs.Toast(message);

            return (addContactResult, newContact.Id);
        }

        private byte[] GetMetaDataForFile()
        {
            //add sender id so it's clear how to decrypt
            var senderId = myId.ToByteArray();
            return senderId; //16 bytes
        }

        private (byte[] encryptedData, byte[] metaData) ReadMetaDataFromFile(byte[] file)
        {
            return (file.Take(guidSizeBytes).ToArray(), file.Skip(guidSizeBytes).ToArray());
        }

        private Contact GetContactWithId(Guid id)
        {
            var contacts = storageService.LoadContacts();
            return contacts.FirstOrDefault(c => c.Id == id);
        }

        private (X25519PublicKeyParameters key, Guid keyId, Guid justScannedId) DecodePublicKey(string publicKeyEncoded)
        {
            try
            { 
                var bytes = Convert.FromBase64String(publicKeyEncoded);
                var keyId = new Guid(bytes.Take(guidSizeBytes).ToArray());
                var justScannedId = new Guid(bytes.Skip(guidSizeBytes).Take(guidSizeBytes).ToArray());
                var keyBytes = bytes.Skip(guidSizeBytes).Skip(guidSizeBytes).ToArray();
                var publicKey = new X25519PublicKeyParameters(keyBytes);
                var sharedSecret = GetSharedSecret(myPrivateKey, publicKey);
                return (publicKey, keyId, justScannedId);
            }
            catch(Exception e)
            {
                return (null, default, default);
            }
        }

        private byte[] GetSharedSecret(AsymmetricKeyParameter myPrivateKey, AsymmetricKeyParameter opponentPublicKey)
        {
            var keyAgreement = new X25519Agreement();
            keyAgreement.Init(myPrivateKey);
            byte[] sharedSecret = new byte[keyAgreement.AgreementSize];
            keyAgreement.CalculateAgreement(opponentPublicKey, sharedSecret, 0);

            Console.WriteLine(Hex.ToHexString(sharedSecret));

            return sharedSecret;
        }

        private (X25519PrivateKeyParameters privateKey, X25519PublicKeyParameters publicKey) GenerateKeyPair()
        {
            keyGen.Init(new KeyGenerationParameters(new Org.BouncyCastle.Security.SecureRandom(), 256));
            var pair = keyGen.GenerateKeyPair();
            return (pair.Private as X25519PrivateKeyParameters, pair.Public as X25519PublicKeyParameters);
        }

        private void GetKeys()
        {
            var keys = storageService.GetMyEncryptionKeys();
            if (keys == null)
            {
                //generate a random ID
                var randomIdBytes = new byte[16];
                random.NextBytes(randomIdBytes);
                myId = new Guid(randomIdBytes);

                (myPrivateKey, myPublicKey) = GenerateKeyPair();
                storageService.SaveMyEncryptionKeys(myPrivateKey, myPublicKey, myId);

                return;
            }

            myPrivateKey = new X25519PrivateKeyParameters(Convert.FromBase64String(keys.PrivateKeyBase64));
            myPublicKey = new X25519PublicKeyParameters(Convert.FromBase64String(keys.PublicKeyBase64));
            myId = new Guid(keys.Id);
        }

        private byte[] Encrypt(byte[] key, byte[] plainTextBytes)
        {
            BufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(engine));
            cipher.Init(true, new KeyParameter(key));
            byte[] rv = new byte[cipher.GetOutputSize(plainTextBytes.Length)];
            int tam = cipher.ProcessBytes(plainTextBytes, 0, plainTextBytes.Length, rv, 0);
            try
            {
                cipher.DoFinal(rv, tam);
            }
            catch (Exception ce)
            {
                Console.WriteLine(ce.StackTrace);
            }
            return rv;
        }

        private byte[] Decrypt(byte[] key, byte[] cipherText)
        {
            BufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(engine));
            cipher.Init(false, new KeyParameter(key));
            byte[] rv = new byte[cipher.GetOutputSize(cipherText.Length)];
            int tam = cipher.ProcessBytes(cipherText, 0, cipherText.Length, rv, 0);
            try
            {
                cipher.DoFinal(rv, tam);
            }
            catch (Exception ce)
            {
                Console.WriteLine(ce.StackTrace);
            }
            return rv;
        }
    }

    public interface IEncryptionService
    {
        /// <summary>
        /// Get data for QR code for pairing devices
        /// </summary>
        /// <param name="justScannedId">The ID of the public key QR code you just scanned OR a GUID filled with zeros by default</param>
        /// <returns></returns>
        string GetMyPublicKeyWithId(Guid justScannedId);

        (AddContactResult result, Guid newContactId) AddPublicKey(string publicKeyEncoded, string contactName);

        Task<string> EncodeFileFor(string filePath, Contact recipientPublicKey);

        Task<string> DecodeFile(string filePath);
    }
}

