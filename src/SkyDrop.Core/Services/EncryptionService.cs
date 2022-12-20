using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Xamarin.Essentials;
using static SkyDrop.Core.Services.EncryptionService;
using static SkyDrop.Core.Utility.Util;
using Contact = SkyDrop.Core.DataModels.Contact;

namespace SkyDrop.Core.Services
{
    /// <summary>
    /// A public key QR code contains this data format:
    /// qrFormatIdentifier [2 bytes]
    /// <- the value should be 1 because we only have one format currently
    ///     nameLength [2 bytes]
    ///     name [ nameLength bytes]
    ///     myId [16 bytes]
    ///     justScannedId [16 bytes]
    ///     publicKey [32 bytes]
    ///     An encrypted file contains this data format:
    ///     headerFormatIdentifier [2 bytes] <- the value should be 1 because we only have one format currently
    ///                                          recipientsCount [2 bytes]
    ///                                          senderId [16 bytes]
    ///                                          recipient1Id [16 bytes]
    ///                                          keyForRecipient1 [64 bytes]
    /// <- the key is encrypted using the
    ///     recipient's public key
    /// recipient2Id [16 bytes]
    /// keyForRecipient2 [64 bytes]
    /// recipient3Id [16 bytes]
    /// keyForRecipient3 [64 bytes]
    /// ...
    /// encryptedData [? bytes] 
    ///     
    /// <- this is encrypted with the key ^
    ///     encryptedData, when decrypted, contains the following data format:
    ///     filenameLength [2 bytes]
    ///     filename [ filenameLength bytes]
    ///     content [? bytes]
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

        private const int GuidSizeBytes = 16;

        private readonly IBlockCipher engine = new ThreefishEngine(256); //the cipher engine for encryption
        private readonly IFileSystemService fileSystemService;

        private readonly IAsymmetricCipherKeyPairGenerator
            keyGen = new X25519KeyPairGenerator(); //keypair generator for X25519 key agreement scheme

        private readonly SecureRandom random = new SecureRandom();
        private readonly IStorageService storageService;

        private readonly IUserDialogs userDialogs;

        private Guid myId;
        private string myName;
        private X25519PrivateKeyParameters myPrivateKey;
        private X25519PublicKeyParameters myPublicKey;

        public EncryptionService(IUserDialogs userDialogs,
            IStorageService storageService,
            IFileSystemService fileSystemService)
        {
            this.userDialogs = userDialogs;
            this.storageService = storageService;
            this.fileSystemService = fileSystemService;

            GetKeys().Forget();
        }

        private Contact MyContact => new Contact { Id = myId, PublicKey = myPublicKey, Name = myName };

        public string GetMyPublicKeyWithId(Guid justScannedId)
        {
            ushort formatId = 1;
            var formatIdBytes = new byte[2]; //these two byes indicate the name length
            BinaryPrimitives.WriteUInt16BigEndian(formatIdBytes, formatId);

            var nameBytes = Encoding.ASCII.GetBytes(myName);
            var nameLength = (ushort)nameBytes.Length;
            var nameLengthBytes = new byte[2]; //these two byes indicate the name length
            BinaryPrimitives.WriteUInt16BigEndian(nameLengthBytes, nameLength);

            var myIdBytes = myId.ToByteArray();
            var justScannedIdBytes = justScannedId.ToByteArray();
            var keyBytes = myPublicKey.GetEncoded();

            var publicKeyWithId = Combine(formatIdBytes, nameLengthBytes, nameBytes, myIdBytes, justScannedIdBytes,
                keyBytes);
            return Convert.ToBase64String(publicKeyWithId);
        }

        public Task<string> EncodeFileFor(string filePath, List<Contact> recipients)
        {
            return Task.Run(() =>
            {
                //allow sender to decrypt their own files
                recipients.Add(MyContact);

                //generate random encryption key
                var encryptionKey = GenerateEncryptionKey(); //32 bytes

                //load the file
                var contentBytes = File.ReadAllBytes(filePath);

                //add the filename to the content
                var fileNameBytes = GetFileNameAsBytes(filePath);
                if (fileNameBytes.Length > ushort.MaxValue)
                    throw new Exception("Filename is too long");

                var fileNameLength = (ushort)fileNameBytes.Length;
                var fileNameLengthBytes = new byte[2]; //these two byes indicate the filename length
                BinaryPrimitives.WriteUInt16BigEndian(fileNameLengthBytes, fileNameLength);
                contentBytes = Combine(fileNameLengthBytes, fileNameBytes, contentBytes);

                //encrypt the file and filename
                var encryptedBytes = Encrypt(encryptionKey, contentBytes);

                //add the metadata
                var metaData = GenerateMetaDataForFile(recipients, encryptionKey);
                var encryptedFileWithMetaData = Combine(metaData, encryptedBytes);

                //save the file
                var randomFileName = GenerateEncryptedFileName(Path.GetFileName(filePath));
                var encryptedFilePath = Path.Combine(fileSystemService.CacheFolderPath, randomFileName);
                File.WriteAllBytes(encryptedFilePath, encryptedFileWithMetaData);

                return encryptedFilePath;
            });
        }

        public Task<string> DecodeFile(string filePath)
        {
            return Task.Run(async () =>
            {
                if (!Path.GetFileName(filePath).IsEncryptedFile())
                    throw new Exception("File not recognised as an encrypted file");

                if (!File.Exists(filePath))
                    throw new Exception("File does not exist");

                var fileBytes = File.ReadAllBytes(filePath);
                var (fileName, file) = GetFilePlainText(fileBytes);

                var saveType = await GetSaveType(fileName);
                if (saveType == SaveType.Cancel)
                    throw new Exception("Action cancelled");

                //save the file
                using var stream = new MemoryStream(file);
                var decryptedFilePath = await fileSystemService.SaveToGalleryOrFiles(stream, fileName, saveType);
                return decryptedFilePath;
            });
        }

        public Task<string> DecodeZipFile(Stream data, string fileName)
        {
            return Task.Run(async () =>
            {
                var (fileName, file) = GetFilePlainText(data.StreamToBytes());

                //save the decrypted zip file in temp folder
                using var stream = new MemoryStream(file);
                var decryptedFilePath = await fileSystemService.SaveFile(stream, fileName, false);
                return decryptedFilePath;
            });
        }

        public (AddContactResult result, Guid newContactId, string existingContactSavedName) AddPublicKey(
            string publicKeyEncoded)
        {
            var (publicKey, keyId, justScannedId, contactName) = DecodePublicKey(publicKeyEncoded);
            if (publicKey == null)
                return (AddContactResult.InvalidKey, default, null);

            var (contactExists, existingContactSavedName) = storageService.ContactExists(keyId);
            if (contactExists)
                return (AddContactResult.AlreadyExists, keyId, existingContactSavedName);

            if (justScannedId != default && justScannedId != myId)
                //you scanned the QR code with the wrong phone
                return (AddContactResult.WrongDevice, default, null);

            var newContact = new Contact { Name = contactName.Trim(), PublicKey = publicKey, Id = keyId };
            storageService.AddContact(newContact);

            var didPair =
                justScannedId ==
                myId; //if justScannedId == default, then we still need to scan this device's QR code to pair
            var message = didPair
                ? $"Paired with {newContact.Name} successfully"
                : $"{newContact.Name} added, now scan this QR code on the other device";
            var addContactResult = didPair ? AddContactResult.DevicesPaired : AddContactResult.ContactAdded;

            userDialogs.Toast(message);

            return (addContactResult, newContact.Id, null);
        }

        public void UpdateDeviceName(string newDeviceName)
        {
            //generate a device name
            var nameMaxLength = 48;
            myName = RemoveNonAsciiChars(newDeviceName);
            myName = myName.Substring(0, Math.Min(nameMaxLength, myName.Length));
            storageService.UpdateDeviceName(myName);
            userDialogs.Toast("Name updated");
        }

        public string GetDeviceName()
        {
            return myName;
        }

        private byte[] GenerateMetaDataForFile(List<Contact> recipients, byte[] encryptionKey)
        {
            if (recipients.Count > ushort.MaxValue)
                throw new Exception($"Exceeded max recipients limit of {ushort.MaxValue}");

            ushort headerFormatIdentifierShort = 1; //increment this if we change the header format
            var headerFormatIdentifier = new byte[2]; // 2 bytes
            BinaryPrimitives.WriteUInt16BigEndian(headerFormatIdentifier, headerFormatIdentifierShort);

            var recipientsCountShort = (ushort)recipients.Count;
            var recipientsCount = new byte[2]; // 2 bytes
            BinaryPrimitives.WriteUInt16BigEndian(recipientsCount, recipientsCountShort);

            //add sender id so it's clear which public key to use for decryption
            var senderId = myId.ToByteArray(); //16 bytes
            var recipientsListBytes = new byte[0];
            foreach (var recipient in recipients)
            {
                var recipientId = recipient.Id.ToByteArray(); //16 bytes
                var sharedSecret = GetSharedSecret(myPrivateKey, recipient.PublicKey);

                //this is the encryption key, encrypted using the recipient's public key
                var recipientKey = Encrypt(sharedSecret, encryptionKey); //64 bytes

                recipientsListBytes = Combine(recipientsListBytes, recipientId, recipientKey);
            }

            return Combine(headerFormatIdentifier, recipientsCount, senderId, recipientsListBytes);
        }

        private (string fileName, byte[] file) GetFilePlainText(byte[] encryptedFile)
        {
            var (metaData, encryptedData) = EncryptedFileMetaData.GetEncryptedFileMetaData(encryptedFile);

            //check if user is listed in file recipients list
            if (!metaData.RecipientKeys.ContainsKey(myId))
                throw new Exception("You are not a valid recipient of this encrypted file");

            //check if user recognises the sender
            var senderPublicKey = GetSenderPublicKey(metaData.SenderId, MyContact);
            if (senderPublicKey == null)
                throw new Exception("The sender of this file is not recognised");

            //find the right key
            var myKeyEncrypted = metaData.RecipientKeys[myId];

            //decrypt the key
            var sharedSecret = GetSharedSecret(myPrivateKey, senderPublicKey);
            var keyPlainTextPadded =
                Decrypt(sharedSecret, myKeyEncrypted); //we only want the first 32 bytes of this 64 byte array
            var keyPlainText = keyPlainTextPadded.Take(32).ToArray();

            //decrypt the file
            var plainTextData = Decrypt(keyPlainText, encryptedData);

            //extract the filename
            var (fileName, file) = ExtractFileName(plainTextData);

            return (fileName, file);
        }

        private X25519PublicKeyParameters GetSenderPublicKey(Guid id, Contact myContact)
        {
            //if I sent this file, decrypt using my own public key
            if (id == myContact.Id)
                return myContact.PublicKey;

            //if someone else sent this file, decrypt using their public key
            var contacts = storageService.LoadContacts();
            return contacts.FirstOrDefault(c => c.Id == id)?.PublicKey;
        }

        private (X25519PublicKeyParameters key, Guid keyId, Guid justScannedId, string name) DecodePublicKey(
            string publicKeyEncoded)
        {
            try
            {
                var bytes = Convert.FromBase64String(publicKeyEncoded);

                var formatIdentifier = BinaryPrimitives.ReadUInt16BigEndian(bytes.Take(2).ToArray());
                if (formatIdentifier != 1)
                    throw new Exception("Invalid public key format");

                var nameLength = BinaryPrimitives.ReadUInt16BigEndian(bytes.Skip(2).Take(2).ToArray());
                var name = Encoding.ASCII.GetString(bytes.Skip(2).Skip(2).Take(nameLength).ToArray());

                var keyId = new Guid(bytes.Skip(2).Skip(2).Skip(nameLength).Take(GuidSizeBytes).ToArray());
                var justScannedId = new Guid(bytes.Skip(2).Skip(2).Skip(nameLength).Skip(GuidSizeBytes)
                    .Take(GuidSizeBytes).ToArray());
                var keyBytes = bytes.Skip(2).Skip(2).Skip(nameLength).Skip(GuidSizeBytes).Skip(GuidSizeBytes).ToArray();
                var publicKey = new X25519PublicKeyParameters(keyBytes);
                var sharedSecret = GetSharedSecret(myPrivateKey, publicKey);
                return (publicKey, keyId, justScannedId, name);
            }
            catch (Exception e)
            {
                return (null, default, default, null);
            }
        }

        private byte[] GetSharedSecret(AsymmetricKeyParameter myPrivateKey, AsymmetricKeyParameter opponentPublicKey)
        {
            var keyAgreement = new X25519Agreement();
            keyAgreement.Init(myPrivateKey);
            var sharedSecret = new byte[keyAgreement.AgreementSize];
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
            keyGen.Init(new KeyGenerationParameters(new SecureRandom(), 256));
            var pair = keyGen.GenerateKeyPair();
            return (pair.Private as X25519PrivateKeyParameters, pair.Public as X25519PublicKeyParameters);
        }

        private async Task GetKeys()
        {
            try
            {
                var keys = await storageService.GetMyEncryptionKeys();
                if (keys == null)
                {
                    //generate a random ID
                    var randomIdBytes = new byte[16];
                    random.NextBytes(randomIdBytes);
                    myId = new Guid(randomIdBytes);

                    //generate a device name
                    var nameMaxLength = 48;
                    myName = RemoveNonAsciiChars(DeviceInfo.Name);
                    myName = myName.Substring(0, Math.Min(nameMaxLength, myName.Length));

                    (myPrivateKey, myPublicKey) = GenerateKeyPair();
                    await storageService.SaveMyEncryptionKeys(myPrivateKey, myPublicKey, myId, myName);

                    return;
                }

                myPrivateKey = new X25519PrivateKeyParameters(Convert.FromBase64String(keys.PrivateKeyBase64));
                myPublicKey = new X25519PublicKeyParameters(Convert.FromBase64String(keys.PublicKeyBase64));
                myId = new Guid(keys.Id);
                myName = keys.Name;
            }
            catch (Exception e)
            {
                var message = e.Message;
                userDialogs.Toast(message);
            }
        }

        private byte[] Encrypt(byte[] key, byte[] plainTextBytes)
        {
            BufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(engine));
            cipher.Init(true, new KeyParameter(key));
            var rv = new byte[cipher.GetOutputSize(plainTextBytes.Length)];
            var tam = cipher.ProcessBytes(plainTextBytes, 0, plainTextBytes.Length, rv, 0);
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
            var rv = new byte[cipher.GetOutputSize(cipherText.Length)];
            var tam = cipher.ProcessBytes(cipherText, 0, cipherText.Length, rv, 0);
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

        private string GenerateEncryptedFileName(string originalFileName)
        {
            //generate name from Guid, without dashes
            var name = new StringBuilder(Guid.NewGuid().ToString("N"));

            if (GetFileCategory(originalFileName) == FileCategory.Zip)
            {
                //add zi signature to identify skydrop encrypted zip files
                name[15] = 'z';
                name[16] = 'i';
            }
            else
            {
                //add sk signature to identify skydrop encrypted files
                name[15] = 's';
                name[16] = 'k';
            }

            return name.ToString();
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

        (AddContactResult result, Guid newContactId, string existingContactSavedName) AddPublicKey(
            string publicKeyEncoded);

        Task<string> EncodeFileFor(string filePath, List<Contact> recipients);

        Task<string> DecodeFile(string filePath);

        Task<string> DecodeZipFile(Stream data, string filename);

        void UpdateDeviceName(string nameDeviceName);

        string GetDeviceName();
    }
}