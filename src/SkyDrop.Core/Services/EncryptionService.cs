using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;
//using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;

namespace SkyDrop.Core.Services
{
    public class EncryptionService : IEncryptionService
    {
        private IBlockCipher engine = new DesEngine(); //the cipher engine for encryption
        private IAsymmetricCipherKeyPairGenerator keyGen = new X25519KeyPairGenerator(); //keypair generator for X25519 key agreement scheme

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

        public void RunExchange()
        {
            var plainTextMessage = AsciiStringToBytes("Hello Sir,");

            GenerateKeys();

            var sharedSecret = GetSharedSecret(myPrivateKey, opponentPublicKey);
            Console.WriteLine($"Shared secret: {BytesToAsciiString(sharedSecret)}");

            var encryptedMessage = Encrypt(sharedSecret, plainTextMessage);
            Console.WriteLine($"Encrypted message: {BytesToAsciiString(encryptedMessage)}");

            var decryptedMessage = Decrypt(sharedSecret, encryptedMessage);
            Console.WriteLine($"Decrypted message: {BytesToAsciiString(decryptedMessage)}");

            var opponentSharedSecret = GetSharedSecret(opponentPrivateKey, myPublicKey);
            Console.WriteLine($"Opponent shared secret: {BytesToAsciiString(opponentSharedSecret)}");

            var opponentDecryptedMessage = Decrypt(opponentSharedSecret, encryptedMessage);
            Console.WriteLine($"Opponent decrypted message: {BytesToAsciiString(opponentDecryptedMessage)}");
        }

        private static byte[] AsciiStringToBytes(string text)
        {
            return System.Text.Encoding.ASCII.GetBytes(text);
        }

        private static string BytesToAsciiString(byte[] bytes)
        {
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        private AsymmetricKeyParameter myPrivateKey;// = new X25519PrivateKeyParameters(Hex.Decode("a546e36bf0527c9d3b16154b82465edd62144c0ac1fc5a18506a2244ba449ac4"), 0);
        private AsymmetricKeyParameter myPublicKey;
        private AsymmetricKeyParameter opponentPrivateKey;
        private AsymmetricKeyParameter opponentPublicKey;// = new X25519PublicKeyParameters(Hex.Decode("e6db6867583030db3594c1a424b15f7c726624ec26b3353b10a903a6d0ab1c4c"), 0);

        private byte[] GetSharedSecret(AsymmetricKeyParameter myPrivateKey, AsymmetricKeyParameter opponentPublicKey)
        {
            var keyAgreement = new X25519Agreement();
            keyAgreement.Init(myPrivateKey);
            byte[] sharedSecret = new byte[keyAgreement.AgreementSize];
            keyAgreement.CalculateAgreement(opponentPublicKey, sharedSecret, 0);

            Console.WriteLine(Hex.ToHexString(sharedSecret)); // c3da55379de9c6908e94ea4df28d084f32eccf03491c71f754b4075577a28552

            return sharedSecret;
        }

        private (AsymmetricKeyParameter privateKey, AsymmetricKeyParameter publicKey) GenerateKeyPair()
        {
            keyGen.Init(new KeyGenerationParameters(new Org.BouncyCastle.Security.SecureRandom(), 256));
            var pair = keyGen.GenerateKeyPair();
            return (pair.Private, pair.Public);
        }

        private void GenerateKeys()
        {
            (myPrivateKey, myPublicKey) = GenerateKeyPair();
            (opponentPrivateKey, opponentPublicKey) = GenerateKeyPair();
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
        public void RunExchange();
    }
}

