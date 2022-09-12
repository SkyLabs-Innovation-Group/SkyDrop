using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Org.BouncyCastle.Asn1.Cms;
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
using static SkyDrop.Core.Utility.Util;

namespace SkyDrop.Core.Services
{
    /// <summary>
    /// A public key QR code contains this data format:
    /// myId [16 bytes]
    /// justScannedId [16 bytes]
    /// publicKey [32 bytes]
    ///
    /// An encrypted (.skydrop) file contains this data format:
    /// headerFormatIdentifier [2 bytes] <- the value should be 1 because we only have one format currently
    /// recipientsCount [2 bytes]
    /// senderId [16 bytes]
    /// recipient1Id [16 bytes]
    /// keyForRecipient1 [64 bytes] <- the key is encrypted using the recipient's public key
    /// recipient2Id [16 bytes]
    /// keyForRecipient2 [64 bytes]
    /// recipient3Id [16 bytes]
    /// keyForRecipient3 [64 bytes]
    /// ...
    /// encryptedData [? bytes] <- this is encrypted with the key ^
    ///
    /// encryptedData, when decrypted, contains the following data format:
    /// filenameLength [2 bytes]
    /// filename [filenameLength bytes]
    /// encryptedContent [? bytes]
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

        public Task<string> EncodeFileFor(string filePath, List<Contact> recipients)
        {
            return Task.Run(() =>
            {
                //generate random encryption key
                var encryptionKey = GenerateEncryptionKey(); //32 bytes

                //load the file
                var contentBytes = File.ReadAllBytes(filePath);

                //add the filename to the content
                var fileNameBytes = GetFileNameAsBytes(filePath);
                if (fileNameBytes.Length > ushort.MaxValue)
                    throw new Exception("Filename is too long");

                ushort fileNameLength = (ushort)fileNameBytes.Length;
                var fileNameLengthBytes = new byte[2];
                BinaryPrimitives.WriteUInt16BigEndian(fileNameLengthBytes, fileNameLength);
                contentBytes = Combine(fileNameLengthBytes, fileNameBytes, contentBytes);

                //encrypt the file and filename
                var encryptedBytes = Encrypt(encryptionKey, contentBytes);

                //add the metadata
                var metaData = GenerateMetaDataForFile(recipients, encryptionKey);
                var encryptedFileWithMetaData = Util.Combine(metaData, encryptedBytes);

                //save the file
                var randomFileName = Guid.NewGuid();
                var encryptedFilePath = Path.Combine(fileSystemService.CacheFolderPath, $"{randomFileName}.skydrop");
                File.WriteAllBytes(encryptedFilePath, encryptedFileWithMetaData);

                return encryptedFilePath;
            });
        }

        public Task<string> DecodeFile(string filePath)
        {
            return Task.Run(async () =>
            {
                if (!filePath.EndsWith(".skydrop"))
                    throw new Exception("File must have .skydrop extension");

                if (!File.Exists(filePath))
                    throw new Exception("File does not exist");

                var fileBytes = File.ReadAllBytes(filePath);
                var (fileName, file) = GetFilePlainText(fileBytes);

                var saveType = await Util.GetSaveType(fileName);
                if (saveType == SaveType.Cancel)
                    throw new Exception("Action cancelled");

                //save the file
                using var stream = new MemoryStream(file);
                var decryptedFilePath = await fileSystemService.SaveToGalleryOrFiles(stream, fileName, saveType);
                return decryptedFilePath;
            });
        }

        public (AddContactResult result, Guid newContactId, string existingContactSavedName) AddPublicKey(string publicKeyEncoded, string contactName)
        {
            var (publicKey, keyId, justScannedId) = DecodePublicKey(publicKeyEncoded);
            if (publicKey == null)
            {
                userDialogs.Alert("Invalid key");
                return (AddContactResult.InvalidKey, default, null);
            }

            var (contactExists, existingContactSavedName) = storageService.ContactExists(keyId);
            if (contactExists)
                return (AddContactResult.AlreadyExists, default, existingContactSavedName);

            if (justScannedId != default && justScannedId != myId)
            {
                //you scanned the QR code with the wrong phone
                userDialogs.Alert("Unexpected device! Please go back and try to pair again");
                return (AddContactResult.WrongDevice, default, null);
            }

            var newContact = new Contact { Name = contactName.Trim(), PublicKey = publicKey, Id = keyId };
            storageService.AddContact(newContact);

            bool didPair = justScannedId == myId; //if justScannedId == default, then we still need to scan this device's QR code to pair
            string message = didPair ? $"Paired with {newContact.Name} successfully" : $"{newContact.Name} added, now scan this QR code on the other device";
            var addContactResult = didPair ? AddContactResult.DevicesPaired : AddContactResult.ContactAdded;

            userDialogs.Toast(message);

            return (addContactResult, newContact.Id, null);
        }

        private byte[] GenerateMetaDataForFile(List<Contact> recipients, byte[] encryptionKey)
        {
            if (recipients.Count > ushort.MaxValue)
                throw new Exception($"Exceeded max recipients limit of {ushort.MaxValue}");

            ushort headerFormatIdentifierShort = 1; //increment this if we change the header format
            var headerFormatIdentifier = new byte[2]; // 2 bytes
            BinaryPrimitives.WriteUInt16BigEndian(headerFormatIdentifier, headerFormatIdentifierShort);

            ushort recipientsCountShort = (ushort)recipients.Count;
            var recipientsCount = new byte[2]; // 2 bytes
            BinaryPrimitives.WriteUInt16BigEndian(recipientsCount, recipientsCountShort);

            //add sender id so it's clear which public key to use for decryption
            var senderId = myId.ToByteArray(); //16 bytes
            var recipientsListBytes = new byte[0];
            foreach(var recipient in recipients)
            {
                var recipientId = recipient.Id.ToByteArray(); //16 bytes
                var sharedSecret = GetSharedSecret(myPrivateKey, recipient.PublicKey);

                //this is the encryption key, encrypted using the recipient's public key
                var recipientKey = Encrypt(sharedSecret, encryptionKey); //64 bytes

                recipientsListBytes = Util.Combine(recipientsListBytes, recipientId, recipientKey);
            }

            return Util.Combine(headerFormatIdentifier, recipientsCount, senderId, recipientsListBytes); 
        }

        private (string fileName, byte[] file) GetFilePlainText(byte[] encryptedFile)
        {
            var (metaData, encryptedData) = EncryptedFileMetaData.GetEncryptedFileMetaData(encryptedFile);

            //check if user is listed in file recipients list
            if (!metaData.RecipientKeys.ContainsKey(myId))
                throw new Exception("You are not a valid recipient of this encrypted file");

            //check if user recognises the sender
            var sender = GetContactWithId(metaData.SenderId);
            if (sender == null)
                throw new Exception("The sender of this file is not recognised");

            //find the right key
            var myKeyEncrypted = metaData.RecipientKeys[myId];

            //decrypt the key
            var sharedSecret = GetSharedSecret(myPrivateKey, sender.PublicKey);
            var keyPlainTextPadded = Decrypt(sharedSecret, myKeyEncrypted); //we only want the first 32 bytes of this 64 byte array
            var keyPlainText = keyPlainTextPadded.Take(32).ToArray();

            //decrypt the file
            var plainTextData = Decrypt(keyPlainText, encryptedData);

            //extract the filename
            var (fileName, file) = ExtractFileName(plainTextData);

            return (fileName, file);
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

        private byte[] GenerateEncryptionKey()
        {
            //TODO: check if this is correct usage of SecureRandom, or if we should call GenerateSeed first
            var key = new byte[32];
            random.NextBytes(key);
            return key;
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

        private byte[] GetFileNameAsBytes(string filePath)
        {
            var name = RemoveNonAsciiChars(Path.GetFileName(filePath));
            return Encoding.ASCII.GetBytes(name);
        }

        private string RemoveNonAsciiChars(string name)
        {
            return Regex.Replace(name, @"[^\u0000-\u007F]+", string.Empty);
        }

        private (string fileName, byte[] file) ExtractFileName(byte[] plainTextData)
        {
            var fileNameLengthSizeBytes = 2;

            var fileNameLengthBytes = plainTextData.Take(fileNameLengthSizeBytes).ToArray();
            var fileNameLength = BinaryPrimitives.ReadUInt16BigEndian(fileNameLengthBytes);

            var fileNameBytes = plainTextData.Skip(fileNameLengthSizeBytes).Take(fileNameLength).ToArray();
            var fileName = Encoding.ASCII.GetString(fileNameBytes);

            var file = plainTextData.Skip(fileNameLengthSizeBytes).Skip(fileNameLength).ToArray();
            return (fileName, file);
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

        (AddContactResult result, Guid newContactId, string existingContactSavedName) AddPublicKey(string publicKeyEncoded, string contactName);

        Task<string> EncodeFileFor(string filePath, List<Contact> recipients);

        Task<string> DecodeFile(string filePath);
    }
}

