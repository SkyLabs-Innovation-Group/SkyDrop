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
using Org.BouncyCastle.Utilities.Encoders;
using SkyDrop.Core.DataModels;

namespace SkyDrop.Core.Services
{
    public class EncryptionService : IEncryptionService
    {
        private IBlockCipher engine = new DesEngine(); //the cipher engine for encryption
        private IAsymmetricCipherKeyPairGenerator keyGen = new X25519KeyPairGenerator(); //keypair generator for X25519 key agreement scheme
        private X25519PrivateKeyParameters myPrivateKey;
        private X25519PublicKeyParameters myPublicKey;

        private readonly IUserDialogs userDialogs;
        private readonly IStorageService storageService;

        public EncryptionService(IUserDialogs userDialogs,
                                 IStorageService storageService)
        {
            this.userDialogs = userDialogs;
            this.storageService = storageService;

            GetKeys();
        }

        public string GetMyPublicKey()
        {
            return Convert.ToBase64String(myPublicKey.GetEncoded());
        }

        public Task<string> EncodeFileFor(string filePath, AsymmetricKeyParameter recipientPublicKey)
        {
            return Task.Run(() =>
            {
                //get shared secret
                var sharedSecret = GetSharedSecret(myPrivateKey, recipientPublicKey);

                //load the file
                var fileBytes = File.ReadAllBytes(filePath);

                //encrypt the file
                var encryptedBytes = Encrypt(sharedSecret, fileBytes);

                //save the file
                var encryptedFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(filePath)); //TODO: replace with temp cache path
                File.WriteAllBytes(encryptedFilePath, encryptedBytes);

                return encryptedFilePath;
            });
        }

        public async Task AddPublicKey(string publicKeyEncoded)
        {
            var publicKey = DecodePublicKey(publicKeyEncoded);
            if (publicKey == null)
            {
                userDialogs.Alert("Invalid key");
                return;
            }

            if (storageService.ContactExists(publicKeyEncoded))
            {
                userDialogs.Alert("Contact already exists");
                return;
            }

            var result = await userDialogs.PromptAsync("Enter contact name: ", null, null, null, "David Vorick");
            if (!result.Ok)
                return;

            var newContact = new Contact { Name = result.Value, PublicKey = publicKey };
            storageService.AddContact(newContact);

            userDialogs.Toast($"{newContact.Name} added");
        }

        private X25519PublicKeyParameters DecodePublicKey(string publicKeyEncoded)
        {
            try
            { 
                var bytes = Convert.FromBase64String(publicKeyEncoded);
                var publicKey = new X25519PublicKeyParameters(bytes);
                var sharedSecret = GetSharedSecret(myPrivateKey, publicKey);
                return publicKey;
            }
            catch(Exception e)
            {
                return null;
            }
        }

        private static byte[] AsciiStringToBytes(string text)
        {
            return System.Text.Encoding.ASCII.GetBytes(text);
        }

        private static string BytesToAsciiString(byte[] bytes)
        {
            return System.Text.Encoding.ASCII.GetString(bytes);
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
                (myPrivateKey, myPublicKey) = GenerateKeyPair();
                storageService.SaveMyEncryptionKeys(myPrivateKey, myPublicKey);

                return;
            }

            myPrivateKey = new X25519PrivateKeyParameters(Convert.FromBase64String(keys.PrivateKeyBase64));
            myPublicKey = new X25519PublicKeyParameters(Convert.FromBase64String(keys.PublicKeyBase64));
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
        string GetMyPublicKey();

        //void RunExchange();

        Task AddPublicKey(string publicKeyEncoded);
    }
}

