using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
using Plugin.Permissions.Abstractions;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;

namespace SkyDrop.Core.Services
{
    public class EncryptionService : IEncryptionService
    {
        private IBlockCipher engine = new DesEngine(); //the cipher engine for encryption
        private IAsymmetricCipherKeyPairGenerator keyGen = new X25519KeyPairGenerator(); //keypair generator for X25519 key agreement scheme
        private X25519PrivateKeyParameters myPrivateKey;
        private X25519PublicKeyParameters myPublicKey;
        private Guid myId;
        private readonly SecureRandom random = new SecureRandom();
        private const int metaDataSizeBytes = 16;

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

        public string GetMyPublicKeyWithId()
        {
            var id = myId.ToByteArray();
            var key = myPublicKey.GetEncoded();
            var publicKeyWithId = Util.Combine(id, key);
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
                    userDialogs.Toast("Sent files can only be decrypted by their recipients");

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

        public async Task AddPublicKey(string publicKeyEncoded)
        {
            var (publicKey, id) = DecodePublicKey(publicKeyEncoded);
            if (publicKey == null)
            {
                userDialogs.Alert("Invalid key");
                return;
            }

            if (storageService.ContactExists(id))
            {
                userDialogs.Alert("Contact already exists");
                return;
            }

            var result = await userDialogs.PromptAsync("Enter contact name: ", null, null, null, "name");
            if (!result.Ok)
                return;

            var newContact = new Contact { Name = result.Value, PublicKey = publicKey, Id = id };
            storageService.AddContact(newContact);

            userDialogs.Toast($"{newContact.Name} added");
        }

        private byte[] GetMetaDataForFile()
        {
            //add sender id so it's clear how to decrypt
            var senderId = myId.ToByteArray();
            return senderId; //16 bytes
        }

        private (byte[] encryptedData, byte[] metaData) ReadMetaDataFromFile(byte[] file)
        {
            return (file.Take(metaDataSizeBytes).ToArray(), file.Skip(metaDataSizeBytes).ToArray());
        }

        private Contact GetContactWithId(Guid id)
        {
            var contacts = storageService.LoadContacts();
            return contacts.FirstOrDefault(c => c.Id == id);
        }

        private (X25519PublicKeyParameters key, Guid id) DecodePublicKey(string publicKeyEncoded)
        {
            try
            { 
                var bytes = Convert.FromBase64String(publicKeyEncoded);
                var id = new Guid(bytes.Take(metaDataSizeBytes).ToArray());
                var keyBytes = bytes.Skip(metaDataSizeBytes).ToArray();
                var publicKey = new X25519PublicKeyParameters(keyBytes);
                var sharedSecret = GetSharedSecret(myPrivateKey, publicKey);
                return (publicKey, id);
            }
            catch(Exception e)
            {
                return (null, default);
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
        string GetMyPublicKeyWithId();

        Task AddPublicKey(string publicKeyEncoded);

        Task<string> EncodeFileFor(string filePath, Contact recipientPublicKey);

        Task<string> DecodeFile(string filePath);
    }
}

